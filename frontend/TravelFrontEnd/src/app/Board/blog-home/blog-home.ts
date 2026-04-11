import { ChangeDetectorRef, Component, ElementRef, Input, input, OnInit, ViewChild, AfterViewInit, } from '@angular/core';
import { ArticleData, ArticleResponse } from '../interface/ArticleData';
import { BoardServe } from '../Service/board-serve';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PopularPost } from "../Components/popular-post/popular-post";
import { TagClouds } from "../Components/tag-clouds/tag-clouds";
import { ArticleList } from "../Components/article-list/article-list";
import { CreateArticleButton } from "../Components/create-article-button/create-article-button";
import { PostCatgories } from "../Components/post-catgories/post-catgories";
import { PageNumberList } from "../Components/page-number-list/page-number-list";
import { DatePipe } from '@angular/common';
import Mandarin from "flatpickr/dist/l10n/zh-tw"
import flatpickr from 'flatpickr';
import { BoardBanner } from "../Components/board-banner/board-banner";
import Swal from 'sweetalert2';
declare var $: any;
@Component({
  selector: 'app-blog-home',
  standalone: true,
  imports: [RouterModule, FormsModule, PopularPost, TagClouds, ArticleList, CreateArticleButton, PostCatgories, PageNumberList, BoardBanner],
  templateUrl: './blog-home.html',
  styleUrl: './blog-home.css',

})
export class BlogHome implements OnInit, AfterViewInit {
  constructor(private Serve: BoardServe, private route: ActivatedRoute, private router: Router,
    private cdr: ChangeDetectorRef
  ) {

  }
  @ViewChild('datePicker') datePickerEl!: ElementRef;

  articleList: ArticleData[] = history.state.articleList || [];
  totalCount = 0;
  currentPage: number = 1;
  Keyword = "";
  AuthorKeyword = "";
  isSearch = false;
  isFocused = false;
  showAdvancedSearch = false;
  private fpInstance: flatpickr.Instance | null = null;
  startdate?: string;
  enddate?: string;

  regions: any[] = [];
  selectedCityId: number | null = null;
  selectedRegionID: number | null = null;
  searchAuthors: any[] = [];

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const hasParams = Object.keys(params).length > 0;
      if (!hasParams) {
        // 沒有參數，正常載入
        this.ReflashArticles(this.currentPage);
        return;
      }
      const dto = {
        page: params['page'] ?? 1,
        Keyword: params['Keyword'] ?? null,
        authorKeyword: params['authorKeyword'] ?? null,
        StartTime: params['StartTime'] ?? null,
        EndTime: params['EndTime'] ?? null,
        RegionId: params['RegionId'] ? Number(params['RegionId']) : null
      };

      this.Serve.getArticleByAllSearch(dto).subscribe((d: any) => {
        this.articleList = d.articleList;
        this.totalCount = d.totalCount;
        this.isSearch = true;
        this.Serve.getAuthorsbyKeyword(dto.authorKeyword).subscribe((d: any) => this.searchAuthors = d);
      });
    });

    // this.route.queryParams.subscribe(params => {
    //   console.log('queryParams triggered', params);
    //   const tagId = params['TagsId'];
    //   if (tagId) {
    //     this.isSearch = true;
    //     const para: string[] = [];
    //     para.push(`TagsId=${tagId}`);
    //     this.Serve.getArticleByTags(1, false, para).subscribe((d: any) => {
    //       this.articleList = d.articleList;
    //       this.totalCount = d.totalCount;
    //       this.cdr.markForCheck();
    //     });
    //   } else {
    //     this.ReflashArticles(this.currentPage);
    //   }
    // });

    this.initRegion();//初始化地區選項

  }
  ngAfterViewInit() {
    flatpickr(this.datePickerEl.nativeElement, {
      dateFormat: 'Y-m-d',
      mode: 'range',
      locale: Mandarin.zh_tw,
      onChange: (selectedDates, dateStr) => {
        const parts = dateStr.split(' 至 ');
        this.startdate = parts[0] ?? null;
        this.enddate = parts[1] ?? null;
      }
    });
    this.cdr.detectChanges();

  }

  onSeachReset() {
    this.fpInstance?.clear();
    const input = document.querySelector('.flatpickr-input') as HTMLInputElement;
    if (input) input.value = '';
    this.Keyword = '';
    this.AuthorKeyword = '';
    this.startdate = undefined;
    this.enddate = undefined;
    this.selectedCityId = null;
    this.selectedRegionID = null;
    this.searchAuthors = [];
  }

  ReflashArticles(currentPage: number) {
    this.Serve.getArticleAPI(currentPage).subscribe(
      {
        next: (d: ArticleResponse) => {
          this.articleList = d.articleList;
          this.totalCount = d.totalCount;
          this.isSearch = false;
          console.log("totalCount", this.totalCount);

        },
        error: (err: any) => {
          if (err.status === 404) {
            this.router.navigate(['Board/404']);
          }
          if (err.status === 401) {
            Swal.fire({
              icon: "warning",
              title: "請先登入",
              timer: 1500
            });
            this.router.navigate(['/login']);
          }
        }
      });
  }



  onSeach() {
    this.router.navigate(['/Board'], {
      queryParams: {
        page: 1,
        Keyword: this.Keyword,
        authorKeyword: this.AuthorKeyword,
        StartTime: this.startdate,
        EndTime: this.enddate,
        RegionId:
          this.selectedRegionID ?? this.selectedCityId ?? null
      }
    });
  }

  onBackSeach() {
    this.searchAuthors = [];
    this.router.navigate(['/Board']);
  }

  onFocus(event: any) {
    this.isFocused = true;
    event.target.closest('.search-box').classList.add('focused');
    if (event.target.value) {
      this.searchByKeyword(event);
    }
  }

  searchByKeyword(event: any) {
    const keyword = event.target.value;
    if (keyword == "") {
      this.ReflashArticles(1);
    }
    else {
      this.Serve.getArticleByKeyword(1, keyword).subscribe((d: any) => {
        this.articleList = d.articleList;
        this.totalCount = d.totalCount;
      });
    }
    this.Keyword = keyword;
  }

  onBlur(event: any) {
    this.isFocused = false;
    setTimeout(() => {
      event.target.closest('.search-box').classList.remove('focused');
    }, 200);
  }

  onTagSelected(tag: any) {
    const para = [`TagsId=${tag}`];
    this.Serve.getArticleByTags(1, false, para).subscribe((d: any) => {
      this.articleList = [...d.articleList];
      this.totalCount = d.totalCount;
      this.isSearch = true;
    });
  }

  get districts() {
    return this.regions?.find(r => r.regionId == Number(this.selectedCityId))?.dist ?? [];
  }

  initRegion() {
    this.Serve.getAllRegions().subscribe((d: any) => {
      this.regions = d;
    });
  }
  get visibleAuthors() {
    const list = this.searchAuthors;
    const start = this.carouselPage * 5;
    return list.slice(start, start + 5);
  }

  carouselPage = 0;
  toggleAdvancedSearch() {
    this.showAdvancedSearch = !this.showAdvancedSearch;
  }


  goToMemderPage(memderID: string): void {
    this.router.navigate(['Board', 'user', memderID]);
  }

  prevPage() {
    if (this.carouselPage > 0) this.carouselPage--;
  }

  nextPage() {
    if ((this.carouselPage + 1) * 5 < this.searchAuthors.length) this.carouselPage++;
  }
}


