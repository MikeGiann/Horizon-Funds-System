import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { forkJoin, Subject, takeUntil } from 'rxjs';
import { SignalrService } from '../services/signalr.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit, OnDestroy {
  transactionForm: FormGroup;
  private destroy$ = new Subject<void>();
  latestTransaction: any = null;

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private signalrService: SignalrService
  ) {
    this.transactionForm = this.fb.group({
      walletId: [1, Validators.required],
      amount: [100, [Validators.required, Validators.min(1)]],
      idempotencyKey: ['']
    });
  }

  ngOnInit(): void {
    this.signalrService.startConnection();
    this.signalrService.joinWalletGroup('TEST-ACCOUNT-123');

    this.signalrService.transactionStream$.pipe(takeUntil(this.destroy$)).subscribe(transaction => {
      this.latestTransaction = transaction;
    });
  }

  ngOnDestroy(): void {
    this.signalrService.leaveWalletGroup('TEST-ACCOUNT-123');
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSubmit(): void {
    if (this.transactionForm.invalid) return;

    const transactionData = {
      ...this.transactionForm.value,
      idempotencyKey: this.transactionForm.value.idempotencyKey || `key-${Date.now()}`
    };

    this.http.post('http://localhost:5000/api/transactions/process', transactionData).subscribe({
      next: (res) => console.log('Transaction successful', res),
      error: (err) => console.error('Transaction failed', err)
    });
  }

  runConcurrencyStressTest(): void {
    const requests = [];
    for (let i = 0; i < 5; i++) {
      requests.push(this.http.post('http://localhost:5000/api/transactions/process', {
        walletId: 1,
        amount: 10,
        idempotencyKey: `stress-key-${i}-${Date.now()}`
      }));
    }

    forkJoin(requests).subscribe({
      next: (results) => console.log('Stress test completed', results),
      error: (err) => console.error('Stress test failed', err)
    });
  }
}