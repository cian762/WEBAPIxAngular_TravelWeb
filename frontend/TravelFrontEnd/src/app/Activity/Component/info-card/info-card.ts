import { pageInterface } from './../../Interface/pageInterface';
import { paginationInterface } from './../../Interface/paginationInterface';
import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import flatpickr from 'flatpickr';
import { CardInfoService } from '../../Service/card-info-service';
import { CardInfoModel } from '../../Interface/cardInterface';
import { DatePipe } from '@angular/common';


@Component({
  selector: 'app-info-card',
  imports: [DatePipe],
  templateUrl: './info-card.html',
  styleUrl: './info-card.css',
})
export class InfoCard implements AfterViewInit, OnDestroy, OnInit {

  constructor(private router: Router, private cardService: CardInfoService, private cdr: ChangeDetectorRef) { }

  region = ["全部", "北部", "中部", "南部", "東部", "離島"];
  type = ["全部", "傳統祭典", "藝術文化", "自然生態", "美食饗宴", "親子同樂", "戶外挑戰", "手作體驗", "美拍景點", "療育養生"];

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

  ngOnInit(): void {
    this.cardService.getCardInfo().subscribe((res) => {

      this.cardData = res.data;
      console.log(this.cardData);

      this.pageData = {
        pageNumber: res.pageNumber,
        pageSize: res.pageSize,
        totalRecords: res.totalRecords,
        totalPages: res.totalPages
      }
      console.log(this.pageData);
      this.cdr.detectChanges();
    });
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
}
