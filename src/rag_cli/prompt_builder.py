def build_prompt(chunks: list, graph_data: list, question: str) -> str:
    context = "\n".join(chunk[0] for chunk in chunks)
    graph_context = "\n".join(str(row) for row in graph_data)
    return f"""Context from documents:
{context}

Knowledge graph information:
{graph_context}

Question: {question}

Please provide a detailed answer based on the context above.""" 