import psycopg2
import os

conn = psycopg2.connect(
    host=os.environ["POSTGRES_HOST"],
    dbname=os.environ["POSTGRES_DB"],
    user=os.environ["POSTGRES_USER"],
    password=os.environ["POSTGRES_PASSWORD"]
)

def insert_document_chunk(content, vector):
    with conn.cursor() as cur:
        cur.execute(
            "INSERT INTO document_chunks (content, embedding) VALUES (%s, %s)",
            (content, vector)
        )
        conn.commit()

def query_similar_chunks(vector):
    with conn.cursor() as cur:
        cur.execute(
            "SELECT content FROM document_chunks ORDER BY embedding <#> %s LIMIT 5",
            (vector,)
        )
        return cur.fetchall()
