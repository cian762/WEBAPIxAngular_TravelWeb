import { PostDetailDto } from './../interface/PostDetailDto';
import { Component, Input, OnInit } from '@angular/core';
import { BoardServe } from '../board-serve';
import { ActivatedRoute, Router } from '@angular/router';
import { FormArray, FormControl, FormGroup, Validators, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Cloudinary, CloudinaryImage } from '@cloudinary/url-gen';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
declare var cloudinary: any;
declare var $: any;
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
  photoView?: string;
  post?: PostDetailDto;
  photoUrlList: string[] = [];
  selectedIndex: number = 0;

  setIndex(index: number) {
    this.selectedIndex = index;
  }

  ngOnInit(): void {
    this.route.paramMap.subscribe(p => {
      this.id = Number(p.get('id'));
      console.log(this.id);
      this.Serve.getArticleDetailAPI(this.id).subscribe((d) => {
        this.post = d;
        this.FormReset(d);
        console.log(this.post);
        // 2. 通知 jQuery 插件：值變了，請更新 UI 顯示
        // 必須放在 setTimeout 確保 Angular 已經把 value 填入原生 <select>
        // setTimeout(() => {
        //   $('#status-select').niceSelect('update');
        // }, 0);
      });
    });
  }

  form = new FormGroup({
    title: new FormControl(this.post?.title),
    content: new FormControl(this.post?.contents),
    status: new FormControl(),
    regionId: new FormControl(),
    photoUrlList: new FormArray([
      new FormControl(''),
    ]),
  })

  FormReset(post: any) {
    this.form.reset({
      title: post.title,
      content: post.contents,
    });
  }
  test() {

  }

  ngAfterViewInit() {
    const $select = $('#status-select');
    setTimeout(() => {
      if ($select.length > 0) {
        $select.niceSelect();
        console.log('nice-select 已啟動');
      } else {
        console.error('找不到 #status-select，請檢查 ID 是否正確');
      }
    }, 50); // 給 50ms 的緩衝

    // 初始化插件
    $select.niceSelect();
    // 監聽插件的變更事件
    $select.on('change', (event: any) => {
      const newValue = event.target.value;
      // 手動將新值塞回 Angular 表單
      this.form.patchValue({ status: Number(newValue) });
      console.log('當前抓到的值：', this.form.get('status')?.value);
    });

    setTimeout(() => {
      $('.your-class').slick({
        dots: true,
        infinite: true,
        speed: 300,
        slidesToShow: 1,
        adaptiveHeight: true
      });
    }, 0);
    this.tryInitSlick();
  }

  // 4. 當你從 API 拿到資料 (this.post) 要回顯時
  updateSelect() {
    // 必須在資料填入後，通知 jQuery 插件更新外觀
    setTimeout(() => {
      $('#status-select').niceSelect('update');
    }, 0);
  }


  async onCreat() {
    if (this.file) {
      await this.uploadImage(this.file);
    }
    const formValue = this.form.value;

    // 將巢狀的表單資料 攤平成後端要的格式
    const postUpdateDto = {
      id: this.id,
      title: formValue.title ?? null,
      photoUrl: this.photoUrl ?? null,
      status: Number(formValue.status || 0), // 確保一定是數字 0-255，不要給空值
      content: formValue.content ?? null,
      regionId: formValue.regionId ?? null,
      // 直接拿 Array
      photoUrlList: formValue.photoUrlList
    };
    const isSuccess = await this.isUploadSuccess(this.id, postUpdateDto);
    if (isSuccess) {
      alert('成功');
    } else {
      console.log('更新失敗');
    }
  }


  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      this.file = file;
      // 2. 建立 FileReader 物件
      const reader = new FileReader();
      // 3. 定義讀取完成後的動作 (這是一個非同步事件)
      reader.onload = () => {
        // reader.result 包含了 base64 的圖片資料
        this.photoView = reader.result as string;

        // 建立一個臨時的預覽網址 (Blob URL)
        const newPhotoUrl = URL.createObjectURL(file);

        // 把新網址加進陣列 (使用展開運算子確保 Angular 偵測到變化)
        this.photoUrlList = [...this.photoUrlList, newPhotoUrl];

        // 選中最新上傳的那張圖
        this.selectedIndex = this.photoUrlList.length - 1;
      };

      // 4. 開始讀取檔案，並轉換成 Data URL (base64)
      reader.readAsDataURL(file);
    } else {
      // 如果使用者取消選擇，清空預覽
      // this.photoView = null;
      // this.selectedFile = null;
    }
  }

  async isUploadSuccess(id: number, para: any) {
    try {
      // 轉換為 Promise 並等待 API 執行結果
      // 使用 map 將結果轉為 true，若成功執行到這一步代表成功
      const result = await firstValueFrom(
        this.Serve.putPostAPI(this.id, para).pipe()
      );
      return result;

    } catch (error) {
      return false;
    }
  }

  async uploadImage(file: File) {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('upload_preset', this.uploadPreset);

    const url = `https://api.cloudinary.com/v1_1/${this.cloudName}/image/upload`;

    try {
      // 使用 firstValueFrom 將 Observable 轉為 Promise
      // 程式會在這裡停住，直到拿到 API 回傳值
      const res = await firstValueFrom(this.http.post<any>(url, formData));
      this.photoUrl = res.secure_url;
      console.log('上傳成功，網址為：', res.secure_url);

      return res.secure_url;
    } catch (err) {
      console.error('上傳失敗', err);
      throw err;
    }
  }


  tryInitSlick() {
    // 延遲稍微拉長一點點測試 (例如 300ms)
    setTimeout(() => {
      const $target = $('.slider-for');

      // 先在控制台印出來，看看這時候 jQuery 到底有沒有抓到東西
      console.log('目標元素長度:', $target.length);

      if ($target.length > 0) {
        $target.slick({
          slidesToShow: 1,
          asNavFor: '.slider-nav',
          fade: true
        });
        console.log('Slick 初始化成功！');
      } else {
        console.warn('Slick 找不到 .slider-for，重試中...');
        // 如果沒抓到，可以遞迴重試一次
        // this.tryInitSlick();
      }
    }, 300);
  }


}
