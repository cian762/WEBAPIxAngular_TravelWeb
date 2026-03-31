import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-hero-section',
  imports: [CommonModule, RouterModule],
  templateUrl: './hero-section.html',
  styleUrl: './hero-section.css',
})
export class HeroSection {
  // 這裡可以放一些簡單的動畫邏輯或文字變量
  title = 'AI 智慧旅遊，規劃就在一瞬間';
  description = '輸入你的目的地，讓我們的 AI 為你量身打造專屬行程，省去數小時的查資料時間。';
}
