from ..shared.embedding import get_embedding
from ..shared.postgres import query_similar_chunks

def run_query(question):
    embedding = get_embedding(question)
    results = query_similar_chunks(embedding)
    context = "\n".join(r[0] for r in results)
    prompt = f"Context:\n{context}\n\nQ: {question}\nA:"
    return prompt  # Or pass to GPT-4 for answer generation
