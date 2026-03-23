import { pageInterface } from './../../Interface/pageInterface';
import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, OnChanges, OnDestroy, OnInit, resolveForwardRef, ViewChild } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import flatpickr from 'flatpickr';
import { CardInfoService } from '../../Service/card-info-service';
import { CardInfoModel } from '../../Interface/cardInterface';
import { DatePipe, NgClass } from '@angular/common';
import { __classPrivateFieldGet } from 'tslib';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, map, of, Subject, switchMap } from 'rxjs';



@Component({
  selector: 'app-info-card',
  imports: [DatePipe, NgClass, FormsModule, RouterLink],
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

  currentSortParam: string = 'latest';

  sortCollection = [
    { label: 'latest', value: '即將開跑' },
    { label: 'hot', value: '熱門討論' },
    { label: 'rating', value: '深獲好評' },
  ];

  keyword: string = '';
  searchResult: string[] = [];
  private keywordSubject = new Subject<string>();
  selectKeyword: string = "";
  isFocus: boolean = false;

  //當輸入框被滑鼠點下關注時
  onMouseDown() {
    this.isFocus = true;
    console.log('mouseDown的focus', this.isFocus);
  }

  //當輸入框被離開關注時
  onMouseLeave() {
    this.isFocus = false;
    console.log('mouseLeave的focus', this.isFocus);

    if (!this.keyword) {
      this.searchResult = [];
    }
    console.log(this.searchResult);
  }

  //當輸入值發生改變，發 API 拉相關的活動名稱
  OnKeyWordChange(value: string) {
    this.keywordSubject.next(value);
  }


  //壓下相符活動名稱後，清空搜尋結果，並且將輸入框名稱補全
  onClickKeyword(res: string) {
    this.selectKeyword = res;
    this.keyword = this.selectKeyword;
    this.searchResult = [];
    //打出 API 找特定活動內容
    this.loadCards(1);
  }



  toggleSortRule(rule: string) {
    this.currentSortParam = rule;
    this.loadCards(1);
  }

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
    this.keywordSubject.pipe(
      map(value => value.trim()),
      debounceTime(500),
      distinctUntilChanged(),
      switchMap(keyword => {
        if (!keyword) {
          this.loadCards(1);
          return of([]);
        }
        return this.cardService.keywordSuggestion(keyword);
      })
    ).subscribe(res => {
      this.searchResult = res;
    });
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


  //傳送挾帶資訊的 API，參數內容要注意
  loadCards(page: number = 1) {
    const query = new queryParameters();
    query.type = this.selectTypes;
    query.region = this.selectRegions;
    query.start = this.formatDate(this.startDate);
    query.end = this.formatDate(this.endDate);
    query.pagenumber = page;
    query.pagesize = 4;
    query.orderbyparam = this.currentSortParam;
    query.keyword = this.keyword;


    console.log('query送出去的內容: ', query);
    this.cardService.FilterCardInfo(query).subscribe((res) => {

      //1. 拿回來的資料分裝進 cardData Model
      this.cardData = res.data;
      console.log(this.cardData);

      //2. 拿回來的資料分裝進 pageData Model
      this.pageData = {
        pageNumber: res.pageNumber,
        pageSize: res.pageSize,
        totalRecords: res.totalRecords,
        totalPages: res.totalPages
      }
      console.log(this.pageData);

      this.currentPage = res.pageNumber;
      this.generatePageNumbers();

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
    this.currentSortParam = 'latest';
    this.keyword = '';


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
  orderbyparam: string = '';
  keyword: string = '';
  pagenumber: number = 1;
  pagesize: number = 8;
}
