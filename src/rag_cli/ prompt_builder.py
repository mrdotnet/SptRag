def build_prompt(context_chunks, graph_facts, question):
    doc_context = "\n".join(context_chunks)
    graph_context = "\n".join([f"{s} {r} {t} ({ctx})" for s, r, t, ctx in graph_facts])
    
    return f"""You are an expert assistant. Use the following context to answer.

Document context:
{doc_context}

Knowledge graph facts:
{graph_context}

Question: {question}
Answer:"""
