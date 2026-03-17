import { pageInterface } from './../../Interface/pageInterface';
import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import flatpickr from 'flatpickr';
import { CardInfoService } from '../../Service/card-info-service';
import { CardInfoModel } from '../../Interface/cardInterface';
import { DatePipe, NgClass } from '@angular/common';
import { __classPrivateFieldGet } from 'tslib';


@Component({
  selector: 'app-info-card',
  imports: [DatePipe, NgClass],
  templateUrl: './info-card.html',
  styleUrl: './info-card.css',
})
export class InfoCard implements AfterViewInit, OnDestroy, OnInit {

  constructor(private router: Router, private cardService: CardInfoService, private cdr: ChangeDetectorRef) { }

  region = ["北部", "中部", "南部", "東部", "離島"];
  type = ["傳統祭典", "藝術文化", "自然生態", "美食饗宴", "親子同樂", "戶外挑戰", "手作體驗", "美拍景點", "療育養生"];

  private fpInstance: flatpickr.Instance | null = null;
  startDate: Date | null = null;
  endDate: Date | null = null;

  @ViewChild('calendarHost', { static: true }) calendarHost!: ElementRef<HTMLElement>;

  cardData: CardInfoModel[] = [];

  pageData: pageInterface = {
    pageNumber: 0,
    pageSize: 0,
    totalPages: 0,
    totalRecords: 0,
  };

  selectTypes: string[] = [];
  selectRegions: string[] = [];

  currentPage: number = 1;
  pageNumbers: number[] = [];




  toggleDefaultChoiceA(): boolean {
    if (this.selectRegions.length > 0) {
      return true;
    }
    return false;
  }
  toggleDefaultChoiceB(): boolean {
    if (this.selectTypes.length > 0) {
      return true;
    }
    return false;
  }

  toggleRegion(item: string) {
    if (this.selectRegions.includes(item)) {
      this.selectRegions = this.selectRegions.filter(x => x !== item);
    }
    else {
      this.selectRegions.push(item);
    }
  };

  toggleType(item: string) {
    if (this.selectTypes.includes(item)) {
      this.selectTypes = this.selectTypes.filter(x => x !== item);
    }
    else {
      this.selectTypes.push(item);
    }
  };

  ngOnInit(): void {
    this.loadCards(1);
  }

  ngAfterViewInit(): void {
    this.fpInstance = flatpickr(this.calendarHost.nativeElement, {
      inline: true,
      mode: 'range',
      showMonths: 2,
      minDate: 'today',
      dateFormat: 'Y-m-d',
      disableMobile: true,
      onChange: (selectedDates) => {
        this.startDate = selectedDates[0] ?? null;
        this.endDate = selectedDates[1] ?? null;
      }
    });
  }
  ngOnDestroy(): void {
    this.fpInstance?.destroy();
  }


  loadCards(page: number = 1) {
    const query = new queryParameters();
    query.type = this.selectTypes;
    query.region = this.selectRegions;
    query.start = this.formatDate(this.startDate);
    query.end = this.formatDate(this.endDate);
    query.pagenumber = page;
    query.pagesize = 8;

    this.cardService.FilterCardInfo(query).subscribe((res) => {
      this.cardData = res.data;
      console.log(this.cardData);

      this.pageData = {
        pageNumber: res.pageNumber,
        pageSize: res.pageSize,
        totalRecords: res.totalRecords,
        totalPages: res.totalPages
      }
      console.log(this.pageData);

      this.currentPage = res.pageNumber;
      this.generatePageNumbers();

      this.cdr.detectChanges();
    });
  }

  //看不懂
  generatePageNumbers() {
    this.pageNumbers = Array.from(
      { length: this.pageData.totalPages },
      (_, i) => i + 1
    );
  }

  //送出搜尋
  submit() {
    this.currentPage = 1;
    this.loadCards(1);
  }

  //清空所有搜尋
  clear() {
    this.selectTypes = [];
    this.selectRegions = [];
    this.startDate = null;
    this.endDate = null;
    this.fpInstance?.clear();

    this.currentPage = 1;
    this.loadCards(1);
  }

  //前往某頁
  goToPage(page: number) {
    if (page < 1 || page > this.pageData.totalPages) return;
    if (page === this.currentPage) return;

    this.currentPage = page;
    this.loadCards(page);
  }

  //前一頁
  prevPage() {
    if (this.currentPage > 1) {
      this.goToPage(this.currentPage - 1);
    }
  }

  //後一頁
  nextPage() {
    if (this.currentPage < this.pageData.totalPages) {
      this.goToPage(this.currentPage + 1);
    }
  }




  formatDate(dateInfo: Date | null) {
    if (!dateInfo) return '';
    const date = String(dateInfo.getDate()).padStart(2, '0');
    const month = String(dateInfo.getMonth() + 1).padStart(2, '0');
    const year = dateInfo.getFullYear();
    return `${year}-${month}-${date}`;
  };
}

export class queryParameters {
  type: string[] = [];
  region: string[] = [];
  start: string = '';
  end: string = '';
  pagenumber: number = 1;
  pagesize: number = 8;
  orderbypopularity: boolean = false;
  islatest: boolean = false;
  isobsolete: boolean = false;
}
