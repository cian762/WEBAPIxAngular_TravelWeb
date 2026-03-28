import {
  Component,
  ElementRef,
  ViewChild,
  HostListener,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CenterEditor } from '../Components/center-editor/center-editor';

export interface ImageItem {
  id: string;
  src: string;
  name: string;
  size: string;
}



@Component({
  selector: 'app-creat-blog',
  standalone: true,
  imports: [CommonModule, FormsModule, CenterEditor],
  templateUrl: './creat-blog.html',
  styleUrl: './creat-blog.css',
})
export class CreatBlog {
  content = "123";
  @ViewChild('editor') editorRef!: ElementRef<HTMLDivElement>;
  @ViewChild('fileInput') fileInputRef!: ElementRef<HTMLInputElement>;
  @ViewChild('listFileInput') listInputRef!: ElementRef<HTMLInputElement>;

  pageTitle = '';
  wordCount = 0;
  lastSaved: string | null = null;

  // ── 右欄圖片列表 ──────────────────────────────────
  imageList: ImageItem[] = [];
  selectedImageId: string | null = null;

  // ── 左欄 Tab ──────────────────────────────────────
  leftTab: 'editor' | 'settings' = 'editor';

  get editor(): HTMLDivElement { return this.editorRef.nativeElement; }

  ngAfterViewInit() { this.editor.focus(); }

  // ── 格式化 ────────────────────────────────────────
  format(command: string, value?: string) {
    this.editor.focus();
    document.execCommand(command, false, value);
  }

  // ── 工具列插入圖片 ────────────────────────────────
  triggerEditorUpload() { this.fileInputRef.nativeElement.click(); }

  onEditorFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    Array.from(input.files).forEach(f => this.readFile(f, true));
    input.value = '';
  }

  // ── 右欄上傳圖片（加入列表）──────────────────────
  triggerListUpload() { this.listInputRef.nativeElement.click(); }

  onListFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    Array.from(input.files).forEach(f => this.readFile(f, false));
    input.value = '';
  }

  private readFile(file: File, insertNow: boolean) {
    if (!file.type.startsWith('image/')) return;
    const reader = new FileReader();
    reader.onload = e => {
      const src = e.target?.result as string;
      const item = this.pushToList(src, file);
      if (insertNow) this.insertImageAtCursor(src, file.name);
      else this.selectedImageId = item.id;
    };
    reader.readAsDataURL(file);
  }

  private pushToList(src: string, file: File): ImageItem {
    const item: ImageItem = {
      id: crypto.randomUUID(),
      src,
      name: file.name,
      size: this.formatBytes(file.size),
    };
    this.imageList.unshift(item);
    return item;
  }

  formatBytes(n: number): string {
    if (n < 1024) return n + ' B';
    if (n < 1048576) return (n / 1024).toFixed(1) + ' KB';
    return (n / 1048576).toFixed(1) + ' MB';
  }

  // ── 點擊右欄圖片 → 插入編輯器 ────────────────────
  insertFromList(item: ImageItem) {
    this.selectedImageId = item.id;
    this.insertImageAtCursor(item.src, item.name);
  }

  removeFromList(id: string, e: MouseEvent) {
    e.stopPropagation();
    this.imageList = this.imageList.filter(i => i.id !== id);
    if (this.selectedImageId === id) this.selectedImageId = null;
  }

  insertImageAtCursor(src: string, alt = '') {
    this.editor.focus();

    const wrapper = document.createElement('div');
    wrapper.className = 'img-wrapper-leftside';


    const img = document.createElement('img');
    img.src = src;
    img.alt = alt;
    img.className = 'inline-img';

    const btn = document.createElement('button');
    btn.textContent = '✕';
    btn.style.cssText = `
    position:absolute; top:-8px; right:-8px;
    width:22px; height:22px; border-radius:50%;
    border:none; background:#e53935; color:white;
    font-size:11px; cursor:pointer; display:none;
  `;
    btn.onclick = () => wrapper.remove();
    wrapper.onmouseenter = () => btn.style.display = 'flex';
    wrapper.onmouseleave = () => btn.style.display = 'none';

    wrapper.appendChild(img);
    wrapper.appendChild(btn);

    const sel = window.getSelection();
    if (sel?.rangeCount) {
      const range = sel.getRangeAt(0);
      range.insertNode(wrapper);
      range.collapse(false);
    }
  }

  // ── 貼上 ─────────────────────────────────────────
  @HostListener('paste', ['$event'])
  onPaste(event: ClipboardEvent) {
    const items = event.clipboardData?.items;
    if (!items) return;
    for (const item of Array.from(items)) {
      if (item.type.startsWith('image/')) {
        event.preventDefault();
        const f = item.getAsFile();
        if (f) this.readFile(f, true);
        return;
      }
    }
    event.preventDefault();
    document.execCommand('insertText', false, event.clipboardData?.getData('text/plain') ?? '');
  }

  // ── 拖放 ─────────────────────────────────────────
  // @HostListener('dragover', ['$event']) onDragOver(e: DragEvent) { e.preventDefault(); this.editor.classList.add('drag-over'); }
  // @HostListener('dragleave') onDragLeave() { this.editor.classList.remove('drag-over'); }
  // @HostListener('drop', ['$event']) onDrop(e: DragEvent) { e.preventDefault(); this.editor.classList.remove('drag-over'); Array.from(e.dataTransfer?.files ?? []).forEach(f => this.readFile(f, true)); }

  updateWordCount() {
    const t = this.editor.innerText ?? '';
    this.wordCount = t.trim() ? t.trim().split(/\s+/).length : 0;
  }

  onEditorInput() { this.updateWordCount(); }

  onEditorKeydown(event: KeyboardEvent) {
    const ctrl = navigator.platform.includes('Mac') ? event.metaKey : event.ctrlKey;
    if (ctrl) {
      const map: Record<string, string> = { b: 'bold', i: 'italic', u: 'underline' };
      if (map[event.key]) { event.preventDefault(); this.format(map[event.key]); }
    }
  }

  saveContent() {
    console.log('儲存：', { title: this.pageTitle, content: this.editor.innerHTML });
    this.lastSaved = new Date().toLocaleTimeString();
  }
}
