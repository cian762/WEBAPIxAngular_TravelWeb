import { CreatBlog } from './../../creat-blog/creat-blog';
import { Component, OnInit } from '@angular/core';
import localeZhTw from '@angular/common/locales/zh-Hant';
import { DatePipe, registerLocaleData } from '@angular/common';
import { BoardServe } from '../../Service/board-serve';
import { ActivatedRoute, Router } from '@angular/router';

registerLocaleData(localeZhTw);

export interface articleSample {
  photoUrl?: string;
  title?: string;
  articleId: number;
  type: number;
  author?: string;
  creatat: Date;
}

@Component({
  selector: 'app-popular-post',
  imports: [DatePipe,],
  templateUrl: './popular-post.html',
  styleUrl: './popular-post.css',
})

export class PopularPost implements OnInit {
  constructor(private Serve: BoardServe, private route: ActivatedRoute, private router: Router) { };
  ngOnInit(): void {
    this.Serve.getActicleByTrending().subscribe(
      (d: any) => {
        this.articleSamples = d;
        console.log(this.articleSamples);
      }
    )
  };


  articleSamples?: articleSample[];
  // {
  //   articleId: 1,
  //   title: "台北夜市美食大搜羅",
  //   cover: "https://res.cloudinary.com/daobwcaga/image/upload/v1774670372/1_ojh1kf.jpg",
  //   creatat: new Date("2026-03-20")
  // },
  // {
  //   articleId: 2,
  //   title: "日月潭的浪漫湖光之旅",
  //   cover: "https://res.cloudinary.com/daobwcaga/image/upload/v1774670372/2_eedahj.jpg",
  //   creatat: new Date("2026-03-21")
  // },
  // {
  //   articleId: 3,
  //   title: "阿里山櫻花季必遊景點",
  //   cover: "https://res.cloudinary.com/daobwcaga/image/upload/v1774666804/1_xqexfp.jpg",
  //   creatat: new Date("2026-03-22")
  // },
  // {
  //   articleId: 4,
  //   title: "墾丁海邊衝浪與夕陽",
  //   cover: "https://res.cloudinary.com/daobwcaga/image/upload/v1774670372/4_glxnsv.jpg",
  //   creatat: new Date("2026-03-23")
  // },
  // {
  //   articleId: 5,
  //   title: "九份老街的懷舊風情與小吃",
  //   cover: "https://res.cloudinary.com/daobwcaga/image/upload/v1774670372/5_unrdlr.jpg",
  //   creatat: new Date("2026-03-24")
  // }

  goToDetail(id: number, type: number): void {
    if (type === 0) {
      this.router.navigate(['Board', 'detail', id]);
    }
    else if (type === 1) {
      this.router.navigate(['Board', 'JournalDetail', id]);
    }
  }

}
