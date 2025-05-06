import logging
import azure.functions as func
from .document_ingestor import ingest_document

def main(req: func.HttpRequest) -> func.HttpResponse:
    try:
        file = req.files['file']
        doc_id = ingest_document(file)
        return func.HttpResponse(f"Document {doc_id} ingested successfully.", status_code=200)
    except Exception as e:
        logging.exception("Ingestion failed.")
        return func.HttpResponse(str(e), status_code=500)
