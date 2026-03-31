import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class CloudinaryServe {
  constructor(private http: HttpClient) { }
  cloudName = "daobwcaga"; // replace with your own cloud name
  uploadPreset = "ml_default"; // replace with your own upload preset

  async uploadImage(file: File) {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('upload_preset', this.uploadPreset);

    const url = `https://api.cloudinary.com/v1_1/${this.cloudName}/image/upload`;

    try {
      // 使用 firstValueFrom 將 Observable 轉為 Promise
      // 程式會在這裡停住，直到拿到 API 回傳值
      const res = await firstValueFrom(this.http.post<any>(url, formData));

      return res.secure_url;
    } catch (err) {
      console.error('上傳失敗', err);
      throw err;
    }
  }
}
