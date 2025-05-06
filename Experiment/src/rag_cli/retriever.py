import psycopg2
import openai
from config import OPENAI_API_KEY, OPENAI_API_BASE, PG_CONFIG, GRAPH_NAME

openai.api_key = OPENAI_API_KEY
openai.api_base = OPENAI_API_BASE

def get_embedding(text):
    result = openai.Embedding.create(input=text, model="text-embedding-ada-002")
    return result['data'][0]['embedding']

def query_pgvector(conn, query_vec, top_k=5):
    with conn.cursor() as cur:
        cur.execute("""
            SELECT content FROM document_chunks
            ORDER BY embedding <#> %s LIMIT %s
        """, (query_vec, top_k))
        return [r[0] for r in cur.fetchall()]

def query_knowledge_graph(conn):
    with conn.cursor() as cur:
        cur.execute(f"""
            SELECT * FROM cypher('{GRAPH_NAME}', $$ 
              MATCH (e)-[r]->(n) 
              RETURN e.name, type(r), n.name, r.context 
            $$) AS (source text, rel text, target text, context text);
        """)
        return cur.fetchall()
