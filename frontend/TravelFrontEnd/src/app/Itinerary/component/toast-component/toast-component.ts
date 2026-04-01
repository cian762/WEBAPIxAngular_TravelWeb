import { Component, inject } from '@angular/core';
import { ToastService } from '../../service/toast-service';

@Component({
  selector: 'app-toast-component',
  imports: [],
  templateUrl: './toast-component.html',
  styleUrl: './toast-component.css',
})
export class ToastComponent {
  toastSvc = inject(ToastService);
}
