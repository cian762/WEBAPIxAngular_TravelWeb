import { Block } from '@angular/compiler';
import { Component } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Component({
  selector: 'app-activity-intro',
  imports: [],
  templateUrl: './activity-intro.html',
  styleUrl: './activity-intro.css',
})
export class ActivityIntro {

  // aside 跳轉到固定的位置，且有設定 offset
  FindBookMark(bookmark: string) {
    console.log("被觸發了");
    const element = document.getElementById(bookmark);
    const offset = 100;

    if (element) {
      const elementPosition = element.getBoundingClientRect().top + window.scrollY;
      const offsetPosition = elementPosition - offset;
      window.scrollTo({ top: offsetPosition, behavior: 'smooth' });
    }
  }





}
