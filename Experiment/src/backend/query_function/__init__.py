# This file makes the directory a Python package
import azure.functions as func
from .query_engine import run_query

def main(req: func.HttpRequest) -> func.HttpResponse:
    question = req.params.get('q')
    result = run_query(question)
    return func.HttpResponse(result, status_code=200)
