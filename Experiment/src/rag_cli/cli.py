import typer
from .retriever import get_embedding, query_pgvector, query_knowledge_graph
from .prompt_builder import build_prompt
import psycopg2
from .config import PG_CONFIG
import openai

app = typer.Typer()

@app.command()
def query(question: str, model: str = "gpt-4"):
    conn = psycopg2.connect(**PG_CONFIG)
    embedding = get_embedding(question)
    chunks = query_pgvector(conn, embedding)
    graph_data = query_knowledge_graph(conn)
    prompt = build_prompt(chunks, graph_data, question)

    response = openai.ChatCompletion.create(
        model=model,
        messages=[{"role": "user", "content": prompt}]
    )

    typer.echo(response['choices'][0]['message']['content'])

@app.command()
def show_context():
    conn = psycopg2.connect(**PG_CONFIG)
    for row in query_knowledge_graph(conn):
        typer.echo(row)

if __name__ == "__main__":
    app()
