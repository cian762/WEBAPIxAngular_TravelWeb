import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { AttractionService } from '../attraction.service';
import { Attraction, AttractionType } from '../attraction.models';

@Component({
  selector: 'app-attraction-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './attraction-list.html',
  styleUrls: ['./attraction-list.css']
})
export class AttractionListComponent implements OnInit {

  customTabs = [
    { label: 'All', icon: '🗺️', typeIds: [] },
    { label: '旅遊景點', icon: '📷', typeIds: [1, 6, 7, 8, 9] },
    { label: '溫泉景點', icon: '♨️', typeIds: [9, 11, 13] },
    { label: '藝文展館', icon: '🏛️', typeIds: [4, 2] },
    { label: '夜市老街', icon: '🌙', typeIds: [10, 2, 4] },
    { label: '古蹟寺廟', icon: '⛩️', typeIds: [2, 5] },
    { label: '遊樂區', icon: '🎡', typeIds: [3, 11, 13] },
  ];
  activeTabIdx = 0;
  attractions: Attraction[] = [];
  types: AttractionType[] = [];
  switchCustomTab(idx: number): void {
    this.activeTabIdx = idx;
    this.activeTypeId = this.customTabs[idx].typeIds[0] ?? 0;
    this.loadAttractions();
  }

  cityName = '';
  regionIds: number[] = [];
  activeTypeId = 0;
  activeTab: 'attractions' | 'activities' | 'itineraries' = 'attractions';
  switchTab(tab: 'attractions' | 'activities' | 'itineraries'): void {
    this.activeTab = tab;
  }
  keyword = '';
  loading = true;

  readonly typeIcons: Record<string, string> = {
    'All': '🗺️', '旅遊景點': '📷', '自然景觀': '🏔️',
    '歷史古蹟': '🏛️', '主題樂園': '🎡', '文化藝術': '🎨',
    '宗教廟宇': '⛩️', '生態體驗': '🌿', '海洋水族': '🐠',
    '山岳步道': '🥾', '溫泉景點': '♨️', '夜市老街': '🌙',
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private svc: AttractionService,
    private cdr: ChangeDetectorRef
  ) { }

  // ─── 城市介紹靜態資料（新增城市只要在這裡加一筆即可）──────────
  private readonly cityIntroMap: Record<string, {
    image: string;
    heading: string;
    paragraphs: string[];
    link?: { label: string; url: string };
  }> = {
      '臺北市': {
        image: 'assets/img/attraction/city/列表/台北.png',
        heading: '人　是臺北最美的風景',
        paragraphs: [
          '從世界最高塔樓（2004～2010 年）到國際級的中華藝術典藏，我們誠摯的邀請您來到這個充滿著對比的城市，現代與傳統相互包容外，再結合了無比活力與親切笑容，臺北定會成為您在亞洲最美好的回憶。',
          '在臺北，您每個所到之處，多樣的文化特質都充沛鼓盪著，雕龍畫棟的廟宇與現代的街道完美吻合，還有許多世界級餐廳隨時提供您最正統的各式中華料理，別忘了，美味的夜市小吃不僅僅帶給您口腹的滿足，更是引領您體驗臺灣生活的理想去處。',
          '臺北的相對性也在都市與自然接壤之間顯現，幾分鐘的車程，您就能在山林環繞下，浸醞於柔潤的溫泉中，還有許多親山步道與公園在城市間穿梭點綴，一日庸碌繁忙後，陪伴著您沉靜心靈。',
          '無論您是短暫停留或是計畫長期拜訪，臺北所蘊藏的多樣性絕對會讓您回味無窮。',
          '來臺北，感受亞洲之美。',
        ],
        link: { label: '臺北旅遊網', url: 'https://www.travel.taipei/' },
      },
      // 其他城市在此新增，格式相同 ↓
      '新北市': {
        image: 'assets/img/attraction/city/列表/新北.jpg',
        heading: '山海之間的城市秘境',
        paragraphs: ['新北市是臺灣本島最北的城市，環繞整個臺北市，總面積達二千多平方公里，海岸線長達120餘公里，幅員遼闊，新北市的東北、北部、西北部均臨海，市內北海岸與東北角海岸擁有豐富的海景奇觀及海岸風光，每逢夏日，海上盡是弄潮張帆的人潮，整條綿延的山海線，銜接成臺北人最鍾愛的黃金旅遊路線。',

          '南部則為中央山脈北端雪山山脈的支稜，山多溪谷多，為新北市內陸地景的一大特色，也是重要的觀光資源。該市風景名勝之多，為全臺之冠，如著名的觀音山風景區、九份、淡水、東北角、野柳、板橋林本源園邸、鶯歌陶瓷、三峽老街等各類風景及名勝古蹟、風光特殊令人無不嚮往。',

          '這裡有名的特產如觀音山的綠竹筍、坪林的茶葉、鶯歌的陶瓷，更是此地獨有的好東西，您不能不到此一探其趣。',

          '更多旅遊資訊，詳見新北市觀光旅遊網。'],

        link: { label: '新北市旅遊網', url: 'https://tour.ntpc.gov.tw/' },
      },
      '基隆市': {
        image: 'assets/img/attraction/city/列表/基隆.jpg',
        heading: '山海之間的城市秘境',
        paragraphs: ['基隆位於臺灣北部，三面環山，僅北面一處有少量平原迎向大海，即為深水谷灣之基隆港。港灣深入市區，水面寬闊，集商港、軍港、漁港於一身，為進入北臺灣門戶。',

          '由於地形受東北季風的影響，基隆的冬天經常多雨，平均溫度僅在攝氏22度左右，素有「雨港之都」之稱。基隆是雨港也是漁港，魚市的漁貨種類繁多，各地遠洋、近海的漁貨種類，當天現撈的鮮活漁貨，應有盡有，崁仔頂的魚市場可說是全臺灣最有活力、有趣的魚鮮拍賣交易場。',

          '在基隆火車站旁的小港碼頭可搭乘遊艇遊基隆港，沿岸景觀頗有可看之處，從大海看陸地山光水色，由各國駛進駛出之豪華客輪、郵輪的繁忙海港風情，湛藍的海水四季與基隆人共同生活，藍色公路之旅是正流行的親水休閒活動。',

          '清光緒年間，為了加強鞏固海防，因而在基隆地區留下了白米甕砲臺、二沙灣砲臺（海門天險）、大武崙砲臺等戰時古蹟，在此遊山訪古，舊時激烈的戰況早已塵封，只見碉堡的雄偉景觀襯以一望無際的藍天碧海美景。',

          '數百年來，基隆出入過無數異國船隻，帶來了多元的文化與新知，是臺灣與國際交流的港口，更是臺灣歷史上重要的文化港都！巨舶輻輳的國際港市、熱鬧豐美的中元祭典、多樣精緻的廟口小吃以及砲臺與隧道、岬角與灣澳、漁港和魚市……，走訪其間，令人流連忘返。'],

        link: { label: '基隆市旅遊網', url: 'https://travel.klcg.gov.tw/' },
      },
      '宜蘭縣': {
        image: 'assets/img/attraction/city/列表/宜蘭.jpg',
        heading: '山海之間的城市秘境',
        paragraphs: ['位處臺灣東北角的宜蘭縣，由於三面背山、一面向海特殊地形，孕育獨特文化與人情味，呈現以三生共構的世外桃源，如今藉由雪山隧道，拉近與臺北不到50分鐘的時間，竭誠歡迎你打開心胸，細心品嚐自然環境的生態旅遊、養生的冷溫泉、豐沛的海洋遊憩資源，油綠的田園、悠閒漫活的生活，這就是水、綠、淨的宜蘭。',
          '宜蘭多元的山林景緻，最具特色的是棲蘭神木園，有百棵超過千年的臺灣原生紅檜與扁柏，每棵巨木依其樹齡對照當代歷史名人命名，在導覽解說時對歷史做一回顧，也形成神木園的特色。',
          '「龜山島」周邊海底溫泉終年冒煙竄出海面，是太平洋罕見的奇觀，附近海域鯨豚數量十分可觀，成群的海豚躍出海面翻滾，就像芭蕾舞群，是完全不同於海洋公園所見的。',
          '臺灣包羅萬象的人氣小吃，像是滷味、包心粉圓、蚵仔煎、當歸羊肉湯等盡在羅東夜市與東門夜市，還有平價服飾店、鞋店等更是逛街購物的市集，是大眾的最愛。',
          '在宜蘭沒有都市的塵囂，在這兒享受宜蘭寧靜的農莊民宿、淳樸好客的農家主人。找個時間，放下繁重工作，親自到宜蘭體驗一點一滴的多樣面貌吧！'],
      },

    };

  get cityIntro() {
    return this.cityIntroMap[this.cityName] ?? null;
  }
  // ──────────────────────────────────────────────────────────────

  isFromTag = false;
  ngOnInit(): void {
    this.route.queryParams.subscribe(p => {
      this.cityName = p['cityName'] || '全部景點';
      this.regionIds = p['regionIds']
        ? String(p['regionIds']).split(',').map(Number)
        : [];

      if (p['typeId']) {
        this.isFromTag = true;
        this.activeTypeId = Number(p['typeId']);
        const idx = this.customTabs.findIndex(t =>
          t.typeIds.includes(this.activeTypeId)
        );
        this.activeTabIdx = idx >= 0 ? idx : 0;
      } else {
        this.isFromTag = false;
        this.activeTypeId = 0;
        this.activeTabIdx = 0;
      }

      this.loadAttractions();
    });
  }

  loadAttractions(): void {
    this.loading = true;
    const tab = this.customTabs[this.activeTabIdx];
    const typeId = this.activeTypeId || tab.typeIds[0] || undefined;
    this.svc.getAttractions({
      regionId: this.regionIds[0] ?? undefined,
      typeId: typeId || undefined,
      keyword: this.keyword || undefined,
    }).subscribe(data => {
      this.attractions = data;
      this.loading = false;
      this.cdr.detectChanges();
    });
  }

  selectType(id: number, name: string): void {
    this.router.navigate(['/attractions/list'], {
      queryParams: {
        regionIds: this.regionIds.join(','),
        cityName: name,
        typeId: id
      }
    });
  }

  onSearch(): void { this.loadAttractions(); }
  getTypeIcon(name: string): string { return this.typeIcons[name] ?? '📍'; }

  getMainImage(a: Attraction): string {
    if (a.mainImage) {
      return `https://localhost:7285${a.mainImage}`;
    }
    return 'assets/img/b1.jpg';
  }

  goToDetail(a: Attraction): void {
    this.router.navigate(['/attractions/detail', a.attractionId]);
  }

  toggleLike(e: Event, a: Attraction): void {
    e.stopPropagation();
    this.svc.toggleLike(a.attractionId).subscribe(res => {
      a.likeCount = res.likeCount;
      a.isLiked = res.liked;
    });
  }
}
