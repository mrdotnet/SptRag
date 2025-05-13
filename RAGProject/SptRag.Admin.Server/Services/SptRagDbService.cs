using System.Linq.Dynamic.Core;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Radzen;
using SptRag.Admin.Server.Data;

namespace SptRag.Admin.Server.Services
{
    public partial class SptRagDbService
    {
        SptRagDbContext Context
        {
           get
           {
             return this.context;
           }
        }

        private readonly SptRagDbContext context;
        private readonly NavigationManager navigationManager;

        public SptRagDbService(SptRagDbContext context, NavigationManager navigationManager)
        {
            this.context = context;
            this.navigationManager = navigationManager;
        }

        public void Reset() => Context.ChangeTracker.Entries().Where(e => e.Entity != null).ToList().ForEach(e => e.State = EntityState.Detached);

        public void ApplyQuery<T>(ref IQueryable<T> items, Query query = null)
        {
            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Filter))
                {
                    if (query.FilterParameters != null)
                    {
                        items = items.Where(query.Filter, query.FilterParameters);
                    }
                    else
                    {
                        items = items.Where(query.Filter);
                    }
                }

                if (!string.IsNullOrEmpty(query.OrderBy))
                {
                    items = items.OrderBy(query.OrderBy);
                }

                if (query.Skip.HasValue)
                {
                    items = items.Skip(query.Skip.Value);
                }

                if (query.Top.HasValue)
                {
                    items = items.Take(query.Top.Value);
                }
            }
        }


        public async Task ExportDocumentsToExcel(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/sptragdb/documents/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/sptragdb/documents/excel(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        public async Task ExportDocumentsToCSV(Query query = null, string fileName = null)
        {
            navigationManager.NavigateTo(query != null ? query.ToUrl($"export/sptragdb/documents/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')") : $"export/sptragdb/documents/csv(fileName='{(!string.IsNullOrEmpty(fileName) ? UrlEncoder.Default.Encode(fileName) : "Export")}')", true);
        }

        partial void OnDocumentsRead(ref IQueryable<Client.Document> items);

        public async Task<IQueryable<Client.Document>> GetDocuments(Query query = null)
        {
            var items = Context.Documents.AsQueryable();


            if (query != null)
            {
                if (!string.IsNullOrEmpty(query.Expand))
                {
                    var propertiesToExpand = query.Expand.Split(',');
                    foreach(var p in propertiesToExpand)
                    {
                        items = items.Include(p.Trim());
                    }
                }

                ApplyQuery(ref items, query);
            }

            OnDocumentsRead(ref items);

            return await Task.FromResult(items);
        }

        partial void OnDocumentGet(Client.Document item);
        partial void OnGetDocumentById(ref IQueryable<Client.Document> items);


        public async Task<Client.Document> GetDocumentById(string id)
        {
            var items = Context.Documents
                              .AsNoTracking()
                              .Where(i => i.Id == id);

 
            OnGetDocumentById(ref items);

            var itemToReturn = items.FirstOrDefault();

            OnDocumentGet(itemToReturn);

            return await Task.FromResult(itemToReturn);
        }

        partial void OnDocumentCreated(Client.Document item);
        partial void OnAfterDocumentCreated(Client.Document item);

        public async Task<Client.Document> CreateDocument(Client.Document document)
        {
            OnDocumentCreated(document);

            var existingItem = Context.Documents
                              .Where(i => i.Id == document.Id)
                              .FirstOrDefault();

            if (existingItem != null)
            {
               throw new Exception("Item already available");
            }            

            try
            {
                Context.Documents.Add(document);
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(document).State = EntityState.Detached;
                throw;
            }

            OnAfterDocumentCreated(document);

            return document;
        }

        public async Task<Client.Document> CancelDocumentChanges(Client.Document item)
        {
            var entityToCancel = Context.Entry(item);
            if (entityToCancel.State == EntityState.Modified)
            {
              entityToCancel.CurrentValues.SetValues(entityToCancel.OriginalValues);
              entityToCancel.State = EntityState.Unchanged;
            }

            return item;
        }

        partial void OnDocumentUpdated(Client.Document item);
        partial void OnAfterDocumentUpdated(Client.Document item);

        public async Task<Client.Document> UpdateDocument(string id, Client.Document document)
        {
            OnDocumentUpdated(document);

            var itemToUpdate = Context.Documents
                              .Where(i => i.Id == document.Id)
                              .FirstOrDefault();

            if (itemToUpdate == null)
            {
               throw new Exception("Item no longer available");
            }
                
            var entryToUpdate = Context.Entry(itemToUpdate);
            entryToUpdate.CurrentValues.SetValues(document);
            entryToUpdate.State = EntityState.Modified;

            Context.SaveChanges();

            OnAfterDocumentUpdated(document);

            return document;
        }

        partial void OnDocumentDeleted(Client.Document item);
        partial void OnAfterDocumentDeleted(Client.Document item);

        public async Task<Client.Document> DeleteDocument(string id)
        {
            var itemToDelete = Context.Documents
                              .Where(i => i.Id == id)
                              .FirstOrDefault();

            if (itemToDelete == null)
            {
               throw new Exception("Item no longer available");
            }

            OnDocumentDeleted(itemToDelete);


            Context.Documents.Remove(itemToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch
            {
                Context.Entry(itemToDelete).State = EntityState.Unchanged;
                throw;
            }

            OnAfterDocumentDeleted(itemToDelete);

            return itemToDelete;
        }
        }
}