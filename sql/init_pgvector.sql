CREATE EXTENSION IF NOT EXISTS vector;

CREATE TABLE document_chunks (
    id SERIAL PRIMARY KEY,
    content TEXT,
    embedding VECTOR(1536)
);

CREATE INDEX ON document_chunks USING hnsw (embedding vector_cosine_ops) WITH (m = 16, ef_search = 128);
