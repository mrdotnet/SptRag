import io
import textract
from ..shared.embedding import get_embedding
from ..shared.postgres import insert_document_chunk

def extract_text(file) -> str:
    return textract.process(file.stream).decode('utf-8')

def ingest_document(file):
    text = extract_text(file)
    chunks = [text[i:i+1000] for i in range(0, len(text), 1000)]
    for chunk in chunks:
        vector = get_embedding(chunk)
        insert_document_chunk(chunk, vector)
    return file.filename
