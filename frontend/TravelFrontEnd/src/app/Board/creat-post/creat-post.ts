import { TagListDTO } from './../../trip/models/tripproduct.model';
import { PostDetailDto } from './../interface/PostDetailDto';
import { Component, Input, OnInit } from '@angular/core';
import { BoardServe } from '../Service/board-serve';
import { ActivatedRoute, Router } from '@angular/router';
import { FormArray, FormControl, FormGroup, Validators, FormsModule, ReactiveFormsModule } from '@angular/forms';
// import { Cloudinary, CloudinaryImage } from '@cloudinary/url-gen';
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
  post?: PostDetailDto;

  tagList: any[] = [];
  removeTagList: any[] = [];
  allTags: any[] = [];
  filteredTags: any[] = [];

  coverUrl?: string;
  photoUrlList: string[] = [];

  fileList: File[] = [];

  photoList: string[] = [];
  selectedIndex: number = 0;
  coverIndex: number = 0;

  regions = [
    {
      name: '北部',
      cities: [
        {
          name: '台北市',
          districts: ['中正區', '大安區', '信義區']
        }
      ]
    }
  ]

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
      });
      this.Serve.getTagsByArticleAPI(this.id).subscribe((d: any) => {
        this.tagList = d;
      });
      this.Serve.getAllTags().subscribe((d: any) => {
        this.allTags = d;
      });

    });
  }

  form = new FormGroup({
    title: new FormControl(this.post?.title),
    content: new FormControl(this.post?.contents),
    status: new FormControl(),
    regionId: new FormControl(),
  })

  FormReset(post: any) {
    this.form.reset({
      title: post.title,
      content: post.contents,
      status: post.status
    });
    this.updateSelect();
    // 使用 ... 把 postPhoto 陣列拆開來，平鋪進去
    this.coverUrl = post.cover;
    const photos = [];
    // 如果封面有值，塞進第一個
    if (post.cover) {
      photos.push(post.cover);
    }
    // 如果其他照片有值且是陣列，展開塞進去
    if (post.postPhoto && post.postPhoto.length > 0) {
      photos.push(...post.postPhoto);
    }
    this.photoUrlList = photos;
    this.photoList = photos;
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
    });
  }

  // 4. 當你從 API 拿到資料 (this.post) 要回顯時
  updateSelect() {
    // 必須在資料填入後，通知 jQuery 插件更新外觀
    setTimeout(() => {
      $('#status-select').niceSelect('update');
    }, 0);
  }


  async onCreat() {
    await this.uploadAllParallel();
    const formValue = this.form.value;
    console.log("this.coverIndex", this.coverIndex);
    this.coverUrl = this.photoUrlList[this.coverIndex];
    console.log("this.coverUrl onCreat", this.coverUrl);
    console.log(this.photoUrlList);
    this.photoUrlList = this.photoUrlList.filter((url, i) => i !== this.coverIndex);
    console.log(this.photoUrlList);
    // 將巢狀的表單資料 攤平成後端要的格式
    const postUpdateDto = {
      id: this.id,
      title: formValue.title ?? null,
      photoUrl: this.coverUrl ?? null,
      status: Number(formValue.status || 0), // 確保一定是數字 0-255，不要給空值
      content: formValue.content ?? null,
      regionId: formValue.regionId ?? null,
      photoUrlList: this.photoUrlList ?? null
    };

    console.log(postUpdateDto);
    const isSuccess = await this.isUploadSuccess(this.id, postUpdateDto);

    if (isSuccess) {
      alert('成功');
    } else {
      console.log('更新失敗');
    }
  }

  removeTag(id: number) {
    var tag = this.tagList.find(tag => tag.tagId === id);
    this.tagList = this.tagList.filter(tag => tag.tagId !== id);
    this.removeTagList.push(tag);
  }

  addTag(id: number) {
    console.log(id);
    var tag = this.allTags.find(tag => tag.tagId === id);
    this.removeTagList = this.removeTagList.filter(tag => tag.tagId !== id);
    this.tagList.push(tag);
    this.filteredTags = [];
  }


  onFileSelected(event: any) {
    const files = Array.from(event.target.files as FileList);
    if (files.length === 0) return;

    // 1. 直接同步產生所有預覽網址 (不需透過 FileReader)
    const newPhotoUrls = files.map(file => URL.createObjectURL(file));

    // 2. 一次更新兩個陣列，保證順序跟選取時一模一樣
    this.fileList = [...this.fileList, ...files];
    this.photoList = [...this.photoList, ...newPhotoUrls];
  }

  async isUploadSuccess(id: number, para: any) {
    try {
      // 轉換為 Promise 並等待 API 執行結果
      // 使用 map 將結果轉為 true，若成功執行到這一步代表成功
      await firstValueFrom(this.Serve.putPostAPI(this.id, para));
      const TagIDList: number[] = this.tagList.map(t => t.tagId);
      await firstValueFrom(this.Serve.postTagsByArticleAPI(this.id, TagIDList));
      return true;
    }
    catch (error) {
      return false;
    }
  }

  async uploadAllParallel() {
    // 1. 建立一個任務清單 (Promises 陣列)
    const uploadTasks = this.fileList.map((f) =>
      this.uploadImage(f)
    );

    try {
      // 2. 啟動所有任務，並等待「全部」完成，得到所有網址
      const allUrls = await Promise.all(uploadTasks);
      // 3. 過濾掉封面，剩下的就是一般圖片網址

      this.photoUrlList = [
        ...this.photoUrlList,
        ...allUrls
      ];
      console.log('所有圖片上傳完成！');

    } catch (err) {
      console.error('其中一張上傳失敗，整個程序停止', err);
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


  selectCover() {
    this.coverIndex = this.selectedIndex;
  }
  onFocus(event: any) {
    event.target.closest('.search-box').classList.add('focused');
    if (event.target.value) {
      this.onSeach(event);
    }
  }

  onSeach(event: any) {
    const keyword = event.target.value;
    this.filteredTags = this.allTags.filter(tag =>
      tag.tagName.includes(keyword)
    );
  }



  onBlur(event: any) {
    setTimeout(() => {
      event.target.closest('.search-box').classList.remove('focused');
      this.filteredTags = [];
    }, 200);
  }
  onInput(event: Event) {
    const el = event.target as HTMLTextAreaElement;
    el.style.height = 'auto';
    el.style.height = Math.min(el.scrollHeight, 800) + 'px';
  }


  selectedRegion: string = '';
  selectedCity: string = '';
  selectedDistrict: string = '';

  filteredCities: any[] = [];
  filteredDistricts: string[] = [];

  onRegionChange(event: Event) {
    const value = (event.target as HTMLSelectElement).value;
    this.selectedRegion = value;
    this.selectedCity = '';
    this.selectedDistrict = '';
    this.filteredDistricts = [];

    const region = this.regions.find(r => r.name === value);
    this.filteredCities = region ? region.cities : [];
  }

  onCityChange(event: Event) {
    const value = (event.target as HTMLSelectElement).value;
    this.selectedCity = value;
    this.selectedDistrict = '';

    const city = this.filteredCities.find(c => c.name === value);
    this.filteredDistricts = city ? city.districts : [];
  }

  onDistrictChange(event: Event) {
    this.selectedDistrict = (event.target as HTMLSelectElement).value;
  }

}
