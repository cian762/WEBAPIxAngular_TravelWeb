import { RowType } from './../Components/center-editor/center-editor';
import { ChangeDetectorRef, Component, ElementRef, inject, ViewChild } from '@angular/core';
import { CdkDrag, CdkDragDrop, CdkDragEnd, CdkDropList, moveItemInArray } from '@angular/cdk/drag-drop';
import { CloudinaryServe } from '../Service/cloudinary-serve';
import { BoardServe } from '../Service/board-serve';
export interface ImageBox {
  id: number;
  type: number;
  content: string;
  width: number,
  height: number,
  x: number;
  y: number;
}

export interface TextBox {
  id: number;
  content: string;
  width: number,
  height: number,
  x: number;
  y: number;
}

export interface ImageBox {
  id: number;
  content: string;
  width: number,
  height: number,
  x: number;
  y: number;
}


// [alt]="item.name"
@Component({
  selector: 'app-creat-journal',
  standalone: true,
  imports: [CdkDrag, CdkDropList],
  templateUrl: './creat-journal.html',
  styleUrl: './creat-journal.css',
})

export class CreatJournal {
  constructor(private Serve: BoardServe, private _Cserve: CloudinaryServe,) { }
  imageFileList: File[] = [];
  itemBox: ImageBox[] = [];
  selectedImageId?: null;
  cdr = inject(ChangeDetectorRef);
  @ViewChild('listFileInput') listInputRef!: ElementRef<HTMLInputElement>;

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.imageFileList.push(file); // 存起來
      const reader = new FileReader();// 畫面讀取
      reader.onload = () => {
        const img = new Image();
        img.src = reader.result as string;

        img.onload = () => {
          let box: ImageBox = {
            id: this.itemBox.length + 1,
            type: 1,
            content: reader.result as string,
            width: img.naturalWidth,
            height: img.naturalHeight,
            x: 0,
            y: 0
          };
          this.itemBox.unshift(box);// 加在陣列開頭，而不是 push
        };

      };
      reader.readAsDataURL(file);
    }

    // this.ImageURL = await this._Cserve.uploadImage(this.selectedImageFile
  }



  startResize(e: MouseEvent, direction: string, imageid: number) {
    console.log("startResize");
    e.preventDefault();
    const box: ImageBox = this.itemBox.find(b => b.id === imageid)!;
    console.log('startDrag', box.x, box.y);
    const startX = e.clientX;
    const startY = e.clientY;
    const startW = box.width;
    const startH = box.height;
    const ratio = startW / startH; // 原始比例

    const onMouseMove = (moveEvent: MouseEvent) => {
      const dx = moveEvent.clientX - startX;
      const dy = moveEvent.clientY - startY;
      console.log('move', box.x, box.y);
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

      box.width = newW;
      box.height = newW / ratio; // 高度永遠跟著比例算

    };

    const onMouseUp = () => {
      document.removeEventListener('mousemove', onMouseMove);
      document.removeEventListener('mouseup', onMouseUp);
    };

    document.addEventListener('mousemove', onMouseMove);
    document.addEventListener('mouseup', onMouseUp);
  }


  startDrag(e: MouseEvent, box: ImageBox) {
    e.preventDefault();

    const startX = e.clientX - box.x;
    const startY = e.clientY - box.y;

    const onMouseMove = (moveEvent: MouseEvent) => {
      box.x = moveEvent.clientX - startX;
      box.y = moveEvent.clientY - startY;
      this.cdr.detectChanges();
    };

    const onMouseUp = () => {
      document.removeEventListener('mousemove', onMouseMove);
      document.removeEventListener('mouseup', onMouseUp);
    };

    document.addEventListener('mousemove', onMouseMove);
    document.addEventListener('mouseup', onMouseUp);
  }

  triggerListUpload(type: number) {
    if (type === 1) {
      this.listInputRef.nativeElement.click();
    }
    else if (type === 0) {
      let box: ImageBox = {
        id: this.itemBox.length + 1,
        type: 0,
        content: "請輸入文字...",
        width: 100,
        height: 100,
        x: 0,
        y: 0
      };
      this.itemBox.unshift(box);// 加在陣列開頭，而不是 push
    }

  }


  removeFromList(id: number, event: Event) {

  }

  drop(event: CdkDragDrop<any[]>) {
    moveItemInArray(this.itemBox, event.previousIndex, event.currentIndex);
  }

  onSave() {
    console.log(this.itemBox);
  }

}

// (cdkDragEnded)="onDragEnd($event)"
//     (click)="onClick(image)"
