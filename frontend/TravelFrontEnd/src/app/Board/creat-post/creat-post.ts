import { TagListDTO } from './../../trip/models/tripproduct.model';
import { PostDetailDto } from './../interface/PostDetailDto';
import { ChangeDetectorRef, Component, Input, OnDestroy, OnInit } from '@angular/core';
import { BoardServe } from '../Service/board-serve';
import { ActivatedRoute, Router } from '@angular/router';
import { FormArray, FormControl, FormGroup, Validators, FormsModule, ReactiveFormsModule } from '@angular/forms';
// import { Cloudinary, CloudinaryImage } from '@cloudinary/url-gen';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { CommonModule } from '@angular/common';

declare var cloudinary: any;
declare var $: any;
@Component({
  selector: 'app-creat-post',
  imports: [FormsModule, ReactiveFormsModule, CommonModule],
  templateUrl: './creat-post.html',
  styleUrl: './creat-post.css',
  standalone: true,
  template: `
    <advanced-image [cldImg]="img"></advanced-image>
  `,
})


export class CreatPost implements OnInit, OnDestroy {
  cloudName = "daobwcaga"; // replace with your own cloud name
  uploadPreset = "ml_default"; // replace with your own upload preset

  constructor(private Serve: BoardServe, private route: ActivatedRoute, private http: HttpClient, private cdr: ChangeDetectorRef) { }
  private observer!: IntersectionObserver;

  navItems = [
    { id: 'sec1' },
    { id: 'sec2' },
    { id: 'sec3' },
    { id: 'sec4' },
    { id: 'sec5' },
    { id: 'sec6' },
  ];

  activeId = 'sec1';

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

  regions: any[] = [];
  selectedRegionId?: number;
  dists: any[] = [];

  form = new FormGroup({
    title: new FormControl(),
    content: new FormControl(),
    status: new FormControl(),
    cityId: new FormControl(),
    distId: new FormControl()
  })

  //選取的圖片
  setIndex(index: number) {
    this.selectedIndex = index;
  }

  ngOnInit(): void {
    this.observer = new IntersectionObserver(entries => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          this.activeId = entry.target.id;
        }
      });
    }, { threshold: 0.5 });

    this.navItems.forEach(item => {
      const el = document.getElementById(item.id);
      if (el) this.observer.observe(el);
    });

    this.route.paramMap.subscribe(p => {
      this.id = Number(p.get('id'));
      console.log(this.id);
      this.Serve.getArticleDetailAPI(this.id).subscribe((d) => {
        this.post = d;
        this.FormReset(d);
        console.log(this.post);
        this.initRegion();//初始化地區選項
      });
      this.Serve.getTagsByArticleAPI(this.id).subscribe((d: any) => {
        this.tagList = d;
      });
      this.Serve.getAllTags().subscribe((d: any) => {
        this.allTags = d;
      });



    });
  }

  scrollTo(id: string) {
    const el = document.getElementById(id);
    if (!el) return;
    const top = el.getBoundingClientRect().top + window.scrollY - 250; // 80 是上欄高度
    window.scrollTo({ top, behavior: 'smooth' });
  }

  ngOnDestroy() {
    this.observer.disconnect();
  }

  ngAfterViewInit() {
    this.cdr.detectChanges();
  }



  FormReset(post: any) {
    this.form.reset({
      title: post.title,
      content: post.contents,
      status: post.status,

    });
    setTimeout(() => {
      $('#status-select').niceSelect('update');
    }, 0);
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



  initNiceSelect() {
    this.bindSelect('status-select', 'status', false);
    this.bindSelect('region-select', 'cityId', true);
    this.bindSelect('dist-select', 'distId', true);
  }

  bindSelect(id: string, controlName: string, isRegion: boolean) {
    const $el = $(`#${id}`);
    if ($el.length === 0) return;

    $el.niceSelect('destroy');
    $el.niceSelect();
    $el.on('change', (event: any) => {
      const val = event.target.value;
      const parsed = val === '' || val === 'null' ? null : Number(val);
      this.form.patchValue({ [controlName]: parsed });

      // cityId 變更後，等 Angular 渲染 dist-select 再初始化
      if (controlName === 'cityId') {
        this.cdr.detectChanges();
        setTimeout(() => this.bindSelect('dist-select', 'distId', true), 0);
      }
    });

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
    //處理地區選項
    if (formValue.distId) {
      this.selectedRegionId = formValue.distId;
    }
    else if (formValue.cityId) {
      this.selectedRegionId = formValue.cityId;
    }


    // 將巢狀的表單資料 攤平成後端要的格式
    const postUpdateDto = {
      id: this.id,
      title: formValue.title ?? null,
      photoUrl: this.coverUrl ?? null,
      status: Number(formValue.status || 0), // 確保一定是數字 0-255，不要給空值
      content: formValue.content ?? null,
      regionId: this.selectedRegionId ?? null,
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
      tag.tagName.includes(keyword) &&
      !this.tagList.some((t: any) => t.tagId === tag.tagId)
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

  initRegion() {
    this.Serve.getAllRegions().subscribe((d: any) => {
      this.regions = d;

      // 先找城市
      const city = this.regions.find(c => c.regionId === this.post?.regionId);

      if (city) {
        // post.regionId 是城市
        this.form.patchValue({ cityId: city.regionId });
      } else {
        // 找區
        const foundCity = this.regions.find(c =>
          c.dist?.some((dist: any) => dist.regionId === this.post?.regionId)
        );

        if (foundCity) {
          this.form.patchValue({
            cityId: foundCity.regionId,
            distId: this.post?.regionId
          });
        }
      }

      // 資料進來後再初始化
      setTimeout(() => {
        this.initNiceSelect();
      }, 0);
    });
  }


}
