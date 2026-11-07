import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  private hubConnection!: signalR.HubConnection;
  private transactionSubject = new BehaviorSubject<any>(null);

  public transactionStream$ = this.transactionSubject.asObservable();

  public startConnection = () => {
    this.hubConnection = new signalR.HubConnectionBuilder()
                            .withUrl('http://localhost:5000/funds') // Ensure port matches API
                            .build();

    this.hubConnection.start()
      .then(() => console.log('SignalR connection started'))
      .catch(err => console.log('Error while starting SignalR connection: ' + err));

    this.hubConnection.on('TransactionProcessed', (data) => {
      this.transactionSubject.next(data);
    });
  }

  public joinWalletGroup(accountNumber: string) {
    this.hubConnection.invoke('JoinWalletGroup', accountNumber)
      .catch(err => console.error('Error joining group: ', err));
  }
  
  public leaveWalletGroup(accountNumber: string) {
    this.hubConnection.invoke('LeaveWalletGroup', accountNumber)
      .catch(err => console.error('Error leaving group: ', err));
  }
}