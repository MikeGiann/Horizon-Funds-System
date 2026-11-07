import asyncio
import random
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from fastapi.middleware.cors import CORSMiddleware

class RiskRequest(BaseModel):
    accountId: str
    amount: float

class RiskResponse(BaseModel):
    accountId: str
    approved: bool
    reason: str | None = None

app = FastAPI(title="Risk Evaluation Service", version="1.0.0")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"], # In production, specify your API's origin
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.post("/api/risk/evaluate", response_model=RiskResponse)
async def evaluate_risk(request: RiskRequest):
    # Simulate network latency
    await asyncio.sleep(random.uniform(0.5, 2.0)) 

    # Simulate strict check for large amounts
    if request.amount > 10000:
        if random.randint(1, 5) == 1: # 20% chance of failure
            raise HTTPException(status_code=503, detail="Risk service temporarily unavailable for high-value transactions.")
            
    if random.randint(1, 100) == 1: # 1% chance of random failure
        raise HTTPException(status_code=503, detail="Risk service temporarily unavailable.")

    return RiskResponse(
        accountId=request.accountId,
        approved=True,
        reason=None
    )

@app.get("/health")
async def health_check():
    return {"status": "healthy"}