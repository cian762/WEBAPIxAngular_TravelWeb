import { RowType } from './../Components/center-editor/center-editor';
import { ChangeDetectorRef, Component, ElementRef, inject, OnInit, ViewChild } from '@angular/core';
import { CdkDrag, CdkDragDrop, CdkDragEnd, CdkDropList, moveItemInArray } from '@angular/cdk/drag-drop';
import { CloudinaryServe } from '../Service/cloudinary-serve';
import { BoardServe } from '../Service/board-serve';
import { JournalElementDTO, JournalUpDateDTO } from '../interface/JournalElementDTO';
import Swal from 'sweetalert2';
import { firstValueFrom } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TagDTO } from '../interface/ArticleData';
export interface ItemBox {
  id: number;
  zIndex: number;
  type: number;
  content: string;
  width: number,
  height: number,
  x: number;
  y: number;
  needFiletoUpload?: File;
  editing?: boolean;
}

declare var $: any;
@Component({
  selector: 'app-creat-journal',
  standalone: true,
  imports: [CdkDrag, CdkDropList, FormsModule, ReactiveFormsModule],
  templateUrl: './creat-journal.html',
  styleUrl: './creat-journal.css',
})

export class CreatJournal implements OnInit {
  constructor(private Serve: BoardServe, private _Cserve: CloudinaryServe, private route: ActivatedRoute, private router: Router,) { }
  id: number = 0;
  imageFileList: File[] = [];
  itemBoxs: ItemBox[] = [];
  selectedId?: number;
  coverFile?: File;
  viewCover?: string;

  regions: any[] = [];
  selectedRegionId?: number;

  cityId?: number;
  distId?: number;

  tagList: TagDTO[] = [];
  removeTagList: any[] = [];
  allTags: any[] = [];
  filteredTags: any[] = [];

  private nextId = 1;

  cdr = inject(ChangeDetectorRef);
  @ViewChild('listFileInput') listInputRef!: ElementRef<HTMLInputElement>;
  journalUpdate: JournalUpDateDTO = {
    title: '',
    cover: undefined,
    regionId: undefined,
    status: 0,
    elements: undefined,
    tags: []
  };



  ngOnInit(): void {
    this.route.paramMap.subscribe(p => {
      this.id = Number(p.get('id'));
      this.Serve.getJournalAPI(this.id).subscribe((d) => {
        this.journalUpdate = d;
        console.log(this.journalUpdate);
        if (this.journalUpdate.tags) {
          this.tagList = this.journalUpdate.tags;
        }
        this.viewCover = this.journalUpdate.cover;
        this.initRegion();//初始化地區選項
        const El = d.elements;
        if (El)
          this.itemBoxs = El.map((El) => ({
            id: this.nextId++,
            zIndex: El.zindex,
            type: El.elementType,
            content: El.content,
            width: El.width,
            height: El.height,
            x: El.posX,
            y: El.posY,
          }));
      });

      this.Serve.getAllTags().subscribe((d: any) => {
        this.allTags = d;
      });
      // this.Serve.getTagsByArticleAPI(this.id).subscribe((d: any) => {
      //   this.tagList = d;
      // });



    });
  }




  //創建圖片物件
  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;

    Array.from(input.files).forEach(file => {
      const reader = new FileReader();
      reader.onload = () => {
        const img = new Image();
        img.src = reader.result as string;
        img.onload = () => {
          this.itemBoxs.forEach(item => item.zIndex++);
          this.itemBoxs.unshift({
            id: this.nextId++,
            zIndex: 1,
            type: 1,
            content: reader.result as string,
            width: Math.min(img.naturalWidth, 400),
            height: Math.round(Math.min(img.naturalWidth, 400) / (img.naturalWidth / img.naturalHeight)),
            x: 0,
            y: 0,
            needFiletoUpload: file
          });
        };
      };
      reader.readAsDataURL(file);
    });

    input.value = '';
  }


  startResize(e: MouseEvent, direction: string, imageid: number) {
    const snap = (val: number) => Math.round(val / 10) * 10;
    e.preventDefault();
    const box: ItemBox = this.itemBoxs.find(b => b.id === imageid)!;

    const startX = e.clientX;
    const startY = e.clientY;
    const startW = box.width;
    const startH = box.height;
    const ratio = startW / startH; // 原始比例

    const onMouseMove = (moveEvent: MouseEvent) => {
      const dx = Math.round(moveEvent.clientX - startX);
      const dy = Math.round(moveEvent.clientY - startY);

      let newW = startW;

      if (direction === 'bottom-right') {
        newW = Math.max(50, startW + dx);
      } else if (direction === 'bottom-left') {
        newW = Math.max(50, startW - dx);
      } else if (direction === 'top-right') {
        newW = Math.max(50, startW + dx);
      } else if (direction === 'top-left') {
        newW = Math.max(50, startW - dx);
      }
      box.width = snap(Math.max(50, newW));
      if (box.type === 1) {
        box.height = snap(Math.round(newW / ratio));// 圖片照比例
      } else {
        box.height = snap(Math.max(50, startH + dy));// 文字自由調整高度
      }

    };

    const onMouseUp = () => {
      document.removeEventListener('mousemove', onMouseMove);
      document.removeEventListener('mouseup', onMouseUp);
    };

    document.addEventListener('mousemove', onMouseMove);
    document.addEventListener('mouseup', onMouseUp);
  }


  startDrag(e: MouseEvent, box: ItemBox) {
    const snap = (val: number) => Math.round(val / 10) * 10;
    e.preventDefault();

    const startX = e.clientX - box.x;
    const startY = e.clientY - box.y;

    const onMouseMove = (moveEvent: MouseEvent) => {
      box.x = snap(moveEvent.clientX - startX);
      box.y = snap(moveEvent.clientY - startY);
      this.cdr.detectChanges();
    };

    const onMouseUp = () => {
      document.removeEventListener('mousemove', onMouseMove);
      document.removeEventListener('mouseup', onMouseUp);
    };

    document.addEventListener('mousemove', onMouseMove);
    document.addEventListener('mouseup', onMouseUp);
  }

  //創建物件(文字)
  triggerListUpload(type: number) {
    if (type === 1) {
      this.listInputRef.nativeElement.click();
    }
    else if (type === 0) {
      this.itemBoxs.forEach(item => item.zIndex++);
      let box: ItemBox = {
        id: this.nextId++,
        zIndex: 1,
        type: 0,
        content: "請輸入文字...",
        width: 100,
        height: 100,
        x: 0,
        y: 0,

      };
      this.itemBoxs.unshift(box);// 加在陣列開頭，而不是 push
    }

  }


  removeFromList(id: number, event: Event) {
    event.stopPropagation();
    this.itemBoxs = this.itemBoxs.filter(item => item.id !== id);
    this.itemBoxs.forEach((item, index) => {
      item.zIndex = index + 1;
    });
  }

  removeCover() {
    this.viewCover = undefined;
  }

  drop(event: CdkDragDrop<any[]>) {
    moveItemInArray(this.itemBoxs, event.previousIndex, event.currentIndex);

    // 重新根據陣列順序分配 zIndex
    this.itemBoxs.forEach((item, index) => {
      item.zIndex = index + 1;
    });
  }

  async onSave() {
    Swal.fire({
      title: "上傳中...",
      allowOutsideClick: false,
      didOpen: () => {
        Swal.showLoading();
      }
    });

    await this.uploadAllParallel();
    const Elements: JournalElementDTO[] = this.itemBoxs.map(item => ({
      page: 1,
      posX: item.x,
      posY: item.y,
      rotation: 0,
      zindex: item.zIndex,
      elementType: item.type,
      content: item.content,
      width: item.width,
      height: item.height,
    }));


    //處理地區選項
    if (this.distId) {
      this.selectedRegionId = this.distId;
    }
    else if (this.cityId) {
      this.selectedRegionId = this.cityId;
    }
    this.journalUpdate.elements = Elements;
    this.journalUpdate.regionId = this.selectedRegionId;
    this.journalUpdate.tags = this.tagList;
    this.journalUpdate.cover = this.viewCover;


    if (this.coverFile) {
      this.journalUpdate.cover =
        await this.uploadImage(this.coverFile);
    }
    console.log(this.journalUpdate);

    this.Serve.putJournalAPI(this.id, this.journalUpdate).subscribe({
      next: () => {
        Swal.fire({
          title: "上傳完成!",
          icon: "success",
          showCancelButton: true,
          confirmButtonText: "返回主頁",
          cancelButtonText: "繼續編輯"
        }).then((result) => {
          if (result.isConfirmed) {
            this.router.navigate(['Board/Main']);
          } else if (result.isDismissed) Swal.close();
        });
      },
      error: (err) => {
        Swal.fire({ title: "儲存失敗", icon: "error" });
        console.error(err);
      }
    });
  }


  async uploadAllParallel() {
    // 1. 分出需要上傳的和已有網址的
    const toUpload = this.itemBoxs.filter(p => p.needFiletoUpload);
    const alreadyUploaded = this.itemBoxs.filter(p => !p.needFiletoUpload);

    try {
      // 2. 並行上傳所有需要上傳的
      const uploadTasks = toUpload.map(p =>
        this.uploadImage(p.needFiletoUpload!).then(url => ({
          id: p.zIndex,
          url
        }))
      );
      const uploadedResults = await Promise.all(uploadTasks);

      // 3. 把上傳結果塞回 photoList 對應的 item
      uploadedResults.forEach(result => {
        const item = this.itemBoxs.find(p => p.zIndex === result.id);
        if (item) {
          item.content = result.url;
          item.needFiletoUpload = undefined;
        }
      });

      // 4. 依照 id 排序後輸出 photoUrlList，順序不亂
      this.itemBoxs.sort((a, b) => a.zIndex - b.zIndex);

      console.log('所有圖片上傳完成！');
    } catch (err) {
      console.error('其中一張上傳失敗，整個程序停止', err);
    }
  }

  async uploadImage(file: File) {
    try {
      var url = await this._Cserve.uploadImage(file);
      console.log(url);
      return url;
    } catch (err) {
      console.error('上傳失敗', err);
      throw err;
    }
  }


  onCoverSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.coverFile = file;
      const reader = new FileReader();// 畫面讀取
      reader.onload = () => {
        const img = new Image();
        img.src = reader.result as string;
        img.onload = () => {
          this.viewCover = img.src;
        };
      };
      reader.readAsDataURL(file);
    }
  }



  onDelete() {
    Swal.fire({
      title: "刪除文章!",
      text: "確定要刪除這篇文章嗎?\n刪除後無法復原",
      icon: "warning",
      showCloseButton: true,
      confirmButtonText: "刪除",
    }).then((result) => {
      if (result.isConfirmed) {
        this.Serve.deleteArticle(this.id).subscribe(() => {
          Swal.fire({
            text: "文章已成功刪除",
            icon: "success",
            showConfirmButton: false,
            timer: 1500
          });
          this.router.navigate(['Board/Main']);
        });
      }
    });
  }



  removeTag(id: number) {
    var tag = this.tagList.find(tag => tag.tagId === id);
    this.tagList = this.tagList.filter(tag => tag.tagId !== id);
    this.removeTagList.push(tag);
  }
  addTag(id: number) {
    var tag = this.allTags.find(tag => tag.tagId === id);
    this.removeTagList = this.removeTagList.filter(tag => tag.tagId !== id);
    this.tagList.push(tag);
    this.filteredTags = [];
    console.log(this.tagList);
  }
  onFocus(event: any) {
    event.target.closest('.search-box').classList.add('focused');
    if (event.target.value) {
      this.onSeach(event);
    }
  }
  onSeach(event: any) {
    console.log("onSeach");
    const keyword = event.target.value;
    this.filteredTags = this.allTags.filter(tag =>
      tag.tagName.includes(keyword) &&
      !(this.tagList ?? []).some((t: any) => t.tagId === tag.tagId));
  }


  onBlur(event: any) {
    setTimeout(() => {
      event.target.closest('.search-box').classList.remove('focused');
      this.filteredTags = [];
    }, 200);
  }




  initRegion() {
    this.Serve.getAllRegions().subscribe((d: any) => {
      this.regions = d;

      const city = this.regions.find(c => c.regionId === this.journalUpdate?.regionId);

      if (city) {
        this.cityId = this.journalUpdate?.regionId;
      } else {
        const foundCity = this.regions.find(c =>
          c.dist?.some((dist: any) => dist.regionId === this.journalUpdate?.regionId)
        );
        if (foundCity) {
          this.cityId = foundCity.regionId;
          this.distId = this.journalUpdate?.regionId;
        }
      }
    });
  }

  get currentDists() {
    return this.regions.find(r => r.regionId === this.cityId)?.dist ?? [];
  }

  onCityChange() {
    this.distId = undefined;
  }
}

// (cdkDragEnded)="onDragEnd($event)"
//     (click)="onClick(image)"
