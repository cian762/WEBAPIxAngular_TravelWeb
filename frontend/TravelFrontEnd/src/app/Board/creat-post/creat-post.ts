import { Component, Input, OnInit } from '@angular/core';
import { BoardServe } from '../board-serve';
import { ActivatedRoute, Router } from '@angular/router';
import { FormArray, FormControl, FormGroup, Validators, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Cloudinary, CloudinaryImage } from '@cloudinary/url-gen';
import { HttpClient } from '@angular/common/http';
declare var cloudinary: any;
@Component({
  selector: 'app-creat-post',
  imports: [FormsModule, ReactiveFormsModule],
  templateUrl: './creat-post.html',
  styleUrl: './creat-post.css',
  standalone: true,
  template: `
    <advanced-image [cldImg]="img"></advanced-image>
  `,
})
export class CreatPost implements OnInit {
  cloudName = "daobwcaga"; // replace with your own cloud name
  uploadPreset = "ml_default"; // replace with your own upload preset

  constructor(private Serve: BoardServe, private route: ActivatedRoute, private http: HttpClient) {

  }
  id = 0;
  photoUrl?: string;
  file?: File;
  ngOnInit(): void {
    this.route.paramMap.subscribe(p => {
      this.id = Number(p.get('id'));
    });
    console.log(this.id);
  }

  form = new FormGroup({
    title: new FormControl(),
    content: new FormControl(),
    status: new FormControl(),
    regionId: new FormControl(),
    photoUrlList: new FormArray([
      new FormControl(''),
    ]),
  })

  test() {

  }

  onCreat() {
    if (this.file) {
      this.uploadImage(this.file);
    }
    const formValue = this.form.value;

    // 將巢狀的表單資料 攤平成後端母類別要的格式
    const postUpdateDto = {
      id: this.id,
      title: formValue.title ?? null,
      photoUrl: this.photoUrl ?? null,
      status: formValue.status,
      content: formValue.content ?? null,
      regionId: formValue.regionId ?? null,
      // 直接拿 Array
      photoUrlList: formValue.photoUrlList
    };


    this.Serve.putPostAPI(this.id, postUpdateDto).subscribe(b => console.log(b));
  }


  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      this.file = file;
    }
  }

  uploadImage(file: File) {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('upload_preset', this.uploadPreset);

    // Cloudinary 的標準上傳 API 網址
    const url = `https://api.cloudinary.com/v1_1/${this.cloudName}/image/upload`;

    this.http.post<any>(url, formData).subscribe({
      next: (res) => {
        // res.secure_url 就是你要存進資料庫的網址
        this.photoUrl = res.secure_url;
        console.log('上傳成功，網址為：', res.secure_url);

        // 如果你有 FormGroup，可以在這裡更新值
        // this.form.patchValue({ imageUrl: res.secure_url });
      },
      error: (err) => {
        console.error('上傳失敗', err);
      }
    });
  }

}
