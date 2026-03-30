import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { CdkDrag, CdkDragEnd, CdkDropList } from '@angular/cdk/drag-drop';
import { CloudinaryServe } from '../Service/cloudinary-serve';
import { BoardServe } from '../Service/board-serve';
export interface ImageBox {
  id: number;
  imageUrl: string;
  width: number,
  height: number,
  x: number;
  y: number;
}

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
  imageboxes: ImageBox[] = [];
  cdr = inject(ChangeDetectorRef);
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
            id: this.imageboxes.length + 1,
            imageUrl: reader.result as string,
            width: img.naturalWidth,
            height: img.naturalHeight,
            x: 0,
            y: 0
          };
          this.imageboxes.push(box);
        };

      };
      reader.readAsDataURL(file);
    }

    // this.ImageURL = await this._Cserve.uploadImage(this.selectedImageFile
  }



  startResize(e: MouseEvent, direction: string, imageid: number) {
    console.log("startResize");
    e.preventDefault();
    const box: ImageBox = this.imageboxes.find(b => b.id === imageid)!;
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

  // onDragEnd(e: CdkDragEnd, box: ImageBox) {
  //   box.x += e.distance.x;
  //   box.y += e.distance.y;
  //   e.source.reset(); // 清掉 transform，改由 left/top 定位
  //   this.cdr.detectChanges();
  // }

  //   onClick(box: ImageBox) {
  //     if (this.isDragging) {
  //       this.isDragging = false;
  //       return;
  //     }
  //     this.selectBox();
  //   }
  //   selectBox() { };
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



}

// (cdkDragEnded)="onDragEnd($event)"
//     (click)="onClick(image)"
