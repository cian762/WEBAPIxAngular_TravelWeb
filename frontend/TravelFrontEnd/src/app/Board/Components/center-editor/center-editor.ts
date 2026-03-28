import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, ElementRef, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';

export type RowType = 'text-full' | 'image-full' | 'image-left' | 'image-right';
export interface Row {
  id: string;
  type: RowType;
  text?: string;   // 文字區塊或半寬文字
  src?: string;    // 圖片 base64
  alt?: string;
}


@Component({
  selector: 'app-center-editor',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './center-editor.html',
  styleUrl: './center-editor.css',
})
export class CenterEditor implements AfterViewInit {
  @ViewChild('fileInput') fileInputRef!: ElementRef<HTMLInputElement>;

  focusedRowIndex = 0;

  rows: Row[] = [
    { id: this.uid(), type: 'text-full', text: '' }
  ];

  ngAfterViewInit() { }

  trackById(_: number, row: Row) { return row.id; }

  uid() { return crypto.randomUUID(); }

  // ── 自動調整 textarea 高度 ──────────────────────
  autoResize(event: Event) {
    const el = event.target as HTMLTextAreaElement;
    el.style.height = 'auto';
    el.style.height = el.scrollHeight + 'px';
  }

  // ── 觸發圖片上傳 ────────────────────────────────
  triggerUpload() {
    this.fileInputRef.nativeElement.click();
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    const file = input.files[0];
    const reader = new FileReader();
    reader.onload = e => {
      this.insertImage(e.target?.result as string, file.name);
    };
    reader.readAsDataURL(file);
    input.value = '';
  }

  // ── 插入圖片（預設全寬，右欄按鈕可切換）─────────
  insertImage(src: string, alt = '', layout: 'full' | 'left' | 'right' = 'left') {
    const idx = this.focusedRowIndex;
    const focusedRow = this.rows[idx];

    // 切割游標所在文字區塊（簡化：直接在後面插入）
    const newRows: Row[] = [];

    if (layout === 'full') {
      newRows.push(
        { id: this.uid(), type: 'image-full', src, alt },
        { id: this.uid(), type: 'text-full', text: '' }  // 圖片下方自動補文字框
      );
    } else if (layout === 'left') {
      newRows.push(
        { id: this.uid(), type: 'image-left', src, alt, text: '' },
        { id: this.uid(), type: 'text-full', text: '' }  // 半寬列下方自動補文字框
      );
    } else {
      newRows.push(
        { id: this.uid(), type: 'image-right', src, alt, text: '' },
        { id: this.uid(), type: 'text-full', text: '' }
      );
    }

    // 插入到 focusedRow 之後
    this.rows.splice(idx + 1, 0, ...newRows);
    this.focusedRowIndex = idx + newRows.length;
  }

  // ── 右欄佈局按鈕：切換選取圖片的排版 ───────────
  setLayout(layout: 'full' | 'left' | 'right') {
    const row = this.rows[this.focusedRowIndex];
    if (!row?.src) return;  // 不是圖片列就不動

    if (layout === 'full') row.type = 'image-full';
    if (layout === 'left') row.type = 'image-left';
    if (layout === 'right') row.type = 'image-right';

    // 確保半寬列有 text 欄位
    if (layout !== 'full' && row.text === undefined) {
      row.text = '';
    }

    // 確保圖片下方有文字框
    const next = this.rows[this.focusedRowIndex + 1];
    if (!next || next.type !== 'text-full') {
      this.rows.splice(this.focusedRowIndex + 1, 0,
        { id: this.uid(), type: 'text-full', text: '' }
      );
    }
  }

  // ── 取得輸出 HTML（儲存用）──────────────────────
  getHTML(): string {
    return this.rows.map(row => {
      if (row.type === 'text-full') {
        return `<p>${row.text}</p>`;
      } else if (row.type === 'image-full') {
        return `<img src="${row.src}" alt="${row.alt}" style="width:100%"/>`;
      } else if (row.type === 'image-left') {
        return `<div style="display:flex"><img src="${row.src}" style="width:50%"/><p style="width:50%">${row.text}</p></div>`;
      } else {
        return `<div style="display:flex"><p style="width:50%">${row.text}</p><img src="${row.src}" style="width:50%"/></div>`;
      }
    }).join('');
  }
}
