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
    { label: 'All', svg: '🗺️', typeIds: [] },
    { label: '旅遊景點', svg: '📸', typeIds: [1, 6, 7, 8, 9] },
    { label: '溫泉景點', svg: '♨️', typeIds: [9, 11, 13] },
    { label: '藝文展館', svg: '🏛️', typeIds: [4, 2] },
    { label: '夜市老街', svg: '🌃', typeIds: [10, 2, 4] },
    { label: '古蹟寺廟', svg: '⛩️', typeIds: [2, 5] },
    { label: '遊樂區', svg: '🎡', typeIds: [3, 11, 13] },
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
        heading: '雨港風情與山海秘境',
        paragraphs: ['基隆位於臺灣北部，三面環山，僅北面一處有少量平原迎向大海，即為深水谷灣之基隆港。港灣深入市區，水面寬闊，集商港、軍港、漁港於一身，為進入北臺灣門戶。',

          '由於地形受東北季風的影響，基隆的冬天經常多雨，平均溫度僅在攝氏22度左右，素有「雨港之都」之稱。基隆是雨港也是漁港，魚市的漁貨種類繁多，各地遠洋、近海的漁貨種類，當天現撈的鮮活漁貨，應有盡有，崁仔頂的魚市場可說是全臺灣最有活力、有趣的魚鮮拍賣交易場。',

          '在基隆火車站旁的小港碼頭可搭乘遊艇遊基隆港，沿岸景觀頗有可看之處，從大海看陸地山光水色，由各國駛進駛出之豪華客輪、郵輪的繁忙海港風情，湛藍的海水四季與基隆人共同生活，藍色公路之旅是正流行的親水休閒活動。',

          '清光緒年間，為了加強鞏固海防，因而在基隆地區留下了白米甕砲臺、二沙灣砲臺（海門天險）、大武崙砲臺等戰時古蹟，在此遊山訪古，舊時激烈的戰況早已塵封，只見碉堡的雄偉景觀襯以一望無際的藍天碧海美景。',

          '數百年來，基隆出入過無數異國船隻，帶來了多元的文化與新知，是臺灣與國際交流的港口，更是臺灣歷史上重要的文化港都！巨舶輻輳的國際港市、熱鬧豐美的中元祭典、多樣精緻的廟口小吃以及砲臺與隧道、岬角與灣澳、漁港和魚市……，走訪其間，令人流連忘返。'],

        link: { label: '基隆市旅遊網', url: 'https://travel.klcg.gov.tw/' },
      },
      '宜蘭縣': {
        image: 'assets/img/attraction/city/列表/宜蘭.jpg',
        heading: '蘭陽平原的漫活世外桃源',
        paragraphs: ['位處臺灣東北角的宜蘭縣，由於三面背山、一面向海特殊地形，孕育獨特文化與人情味，呈現以三生共構的世外桃源，如今藉由雪山隧道，拉近與臺北不到50分鐘的時間，竭誠歡迎你打開心胸，細心品嚐自然環境的生態旅遊、養生的冷溫泉、豐沛的海洋遊憩資源，油綠的田園、悠閒漫活的生活，這就是水、綠、淨的宜蘭。',
          '宜蘭多元的山林景緻，最具特色的是棲蘭神木園，有百棵超過千年的臺灣原生紅檜與扁柏，每棵巨木依其樹齡對照當代歷史名人命名，在導覽解說時對歷史做一回顧，也形成神木園的特色。',
          '「龜山島」周邊海底溫泉終年冒煙竄出海面，是太平洋罕見的奇觀，附近海域鯨豚數量十分可觀，成群的海豚躍出海面翻滾，就像芭蕾舞群，是完全不同於海洋公園所見的。',
          '臺灣包羅萬象的人氣小吃，像是滷味、包心粉圓、蚵仔煎、當歸羊肉湯等盡在羅東夜市與東門夜市，還有平價服飾店、鞋店等更是逛街購物的市集，是大眾的最愛。',
          '在宜蘭沒有都市的塵囂，在這兒享受宜蘭寧靜的農莊民宿、淳樸好客的農家主人。找個時間，放下繁重工作，親自到宜蘭體驗一點一滴的多樣面貌吧！'],
      },
      '桃園市': {
  image: 'assets/img/attraction/city/列表/桃園.jpg',
  heading: '千塘之鄉的多元風采',
  paragraphs: [
    '桃園市擁有多元的文化，加上北橫豐富的山水景觀及「草花王國的故鄉」、「千塘之鄉」等美名，成就桃園為觀光大市，也因這些屬於桃園特別的風華面貌，讓民眾在此相遇，留下了精彩回憶。', // 這裡補上逗號
    '桃園有很多的地方特色等待我們發掘，例如「客家文化」、「眷村體驗」、「農村生活」、「步道」、「鐵馬遊」及「產業體驗」等，我們都將努力發掘更多的在地價值，希望成為下一個吸引遊客的熱門景點，也是下一個「相遇在桃園」的故事。', // 這裡補上逗號
    '更多旅遊資訊，詳見桃園觀光導覽網。'
  ],
   link: { label: '桃園觀光導覽網', url: 'https://travel.tycg.gov.tw/' },
},
'新竹縣': {
        image: 'assets/img/attraction/city/列表/新竹縣.jpg',
        heading: '好客竹縣 歡迎來作客',
        paragraphs: [
          '新竹縣地形多元、氣候溫和、農產豐盛，全縣客家族群約佔總人口數的84%，是典型的客家（Hakka）大縣，並以「好客竹縣」（Hakka Hsinchu）著稱。新竹縣從農業到科技的轉型、從傳統到現代的結合，還有隨著科技發展遷入的新移民，形成多元融合的族群結構與文化特色。',
          '近年來新竹縣積極推展縣內觀光休閒產業，每年持續以「山、湖、海」三大特色景點舉辦觀光季等系列活動，推廣新豐、竹北濱海遊憩區、峨眉湖景點及山地美人湯泉；數百公里山線與海線的自行車道規劃更是提供了民眾休閒運動且落實綠色生活的最佳方式。',
          '在人文古蹟層面上，隨處可見客家農村淳樸的景象，更可循著前人的足跡，藉由名人故居的探訪，走進光陰的迴廊裡，這裡有縣政府用心為您打造一系列的名人故居，如張學良文化園區、蕭如松藝術園區和吳濁流故居等。'
        ],
        link: { label: '新竹縣旅遊網', url: 'https://travel.hsinchu.gov.tw/' },
      },
      '新竹市': {
        image: 'assets/img/attraction/city/列表/新竹市.jpg',
        heading: '風城魅力：科技與古蹟的交響樂',
        paragraphs: [
          '新竹原名竹塹，北從桃園臺地的尾端至鳳山崎，南到香山丘陵，三面環山，僅西面臨海，形成一畚箕地形。是臺灣與中國大陸東南沿海距離最短之處，人口分佈以客家人居多，占總人口數的85％以上，因此在臺灣的客家文化史上扮演舉足輕重的角色。',
          '新竹懷抱著古老的文化，努力展現新的科學城風貌，在新舊交替的背後，潛藏了數百年來風雨飄搖下獨有的魅力，值得細細品嚐。',
          '近十幾年來，由於科學工業園區的設立，標榜著高科技以及勤奮、積極的工作態度，為這個傳統城市帶來不同的思維與衝擊。舊有的文化產業與高科技產業相互撞擊下，發生各種蛻變，讓新竹成為既是文化、教育資源中心，也是重要的科技城市。'
        ],
        link: { label: '新竹市觀光旅遊網', url: 'https://tourism.hccg.gov.tw/' },
      },
      '苗栗縣': {
        image: 'assets/img/attraction/city/列表/苗栗.jpg',
        heading: '山城花園：慢活客莊之旅',
        paragraphs: [
          '苗栗縣位於新竹及臺中之間，不僅氣候宜人、鮮少天災；交通也是相當便利，每年都吸引超過650萬人次的國內、外旅客來苗栗觀光旅遊。',
          '「桐花」、「木雕」、「溫泉」、「水果」、「陶瓷」及「客家菜」是苗栗觀光的6大特色，結合這6項的觀光資源，安排一趟渡假行程，是每位旅客來苗栗旅遊的最佳選擇。',
          '每年的4月及5月，全臺灣最美的油桐花海就會在苗栗盛開，許多旅客喜歡欣賞雪白的桐花遍滿整個山頭，或是倒映在寧靜的湖面上。',
          '苗栗有5個主要的觀光地區，包含：三義木雕及舊山線、大湖草莓文化園區、泰安溫泉、明德水庫及南庄獅頭山；旅客可以再結合客家美食、原住民文化，讓每次的旅程更豐富。'
        ],
        link: { label: '苗栗文化觀光旅遊網', url: 'https://miaolitravel.net/index.aspx' },
      },
      '臺中市': {
        image: 'assets/img/attraction/city/列表/台中.jpg',
        heading: '創意之都：時尚與文化的慢旅',
        paragraphs: [
          '臺中位於臺灣西半部的樞紐位置，四季氣候宜人。臺中市對古蹟的保存不遺餘力，完整保留了日治時期棋盤式街道、具有兩百年歷史的樂成宮、萬和宮、以及大甲鎮瀾宮，在在都讓人發思古之幽情。',
          '現代與傳統相互融合，國立臺灣美術館、國立自然科學博物館與臺中市文化局共築成美學、文化與知識的鐵三角，不僅激盪出對美的感受，更營造出優質的休閒生活！',
          '林立的百貨公司、各有特色的商圈、濃濃歐式風味的精明商圈，以及美術園道的椰林餐廳，都讓臺中市有如巴黎香榭大道的優雅浪漫，滿足所有追求時尚的品味饗宴。'
        ],
        link: { label: '臺中觀光旅遊網', url: 'https://travel.taichung.gov.tw/' },
      },
      '彰化縣': {
        image: 'assets/img/attraction/city/列表/彰化.jpg',
        heading: '臺灣穀倉：古蹟與美食的知性之旅',
        paragraphs: [
          '彰化古稱半線，地勢平坦開闊，物產豐饒，素有「臺灣穀倉」之稱。清領時期鹿港迅速發展，時人並以「一府、二鹿、三艋岬」譽之，便可窺見當時風光景況。',
          '彰化的觀光資源豐富多元，知名的八卦山脈是休閒旅遊的新寵；各鄉鎮美食如彰化肉圓、溪湖羊肉爐、鹿港蚵仔煎、傳統糕餅等，無不令人吮指回味。同時也有許多觀光果園提供農家體驗。',
          '彰化因移民帶來了不同的民情風俗，古蹟面貌豐富，例如彰化地標八卦山大佛、有「臺灣紫禁城」美譽的國定古蹟龍山寺，都是值得遊客一同見證歷史的知性景點。'
        ],
        link: { label: '彰化旅遊資訊網', url: 'https://tourism.chcg.gov.tw/index.aspx' },
      },
      '南投縣': {
        image: 'assets/img/attraction/city/列表/南投.jpg',
        heading: '台灣之心：高山與湖泊的奇景',
        paragraphs: [
          '南投縣位居臺灣本島中央，是全臺灣唯一沒有臨海的縣！以臺灣最高峰——玉山為準點，用41座三千公尺以上的高山綿延出層巒起伏的綠海景觀，臺灣最長的河流濁水溪蜿蜒而過，最美麗的湖泊日月潭點綴其中。',
          '農業為南投縣主要產業，特有的農家風情和豐富的物產塑造了另一種休閒旅遊風，目前縣境內已有數十處規劃完善的休閒農業區，恬靜悠閒的農村旅遊，是闔家渡假的好去處。',
          '這裡旅遊住宿地點多元，從國際級渡假飯店到田園民宿應有盡有，如清境農場充滿異國風情的山莊，或是埔里桃米生態村融合自然生態的雅舍，本身即是一處美景。',
          '無論是春天郊遊、夏天避暑、秋天觀星、還是冬天泡湯玩雪，您會在這裡找到輕鬆、歡樂、自在與平靜的心情！'
        ],
        link: { label: '南投旅遊網', url: 'https://travel.nantou.gov.tw/' },
      },
      '雲林縣': {
        image: 'assets/img/attraction/city/列表/雲林.jpg',
        heading: '農業之鄉：純樸農村與布袋戲故鄉',
        paragraphs: [
          '雲林位於嘉南平原北端，東接南投，西臨台灣海峽。全縣十分之九為平原，自然純樸的農村風光與得天獨厚的農產，一直是雲林人最引以為傲的資源。',
          '倚山傍海的優勢孕育出各式美食伴手禮，例如古坑咖啡、臺西文蛤、口湖馬蹄蛤、斗六文旦、西螺醬油及北港麻油等，每一個都代表了雲林最在地的味道。',
          '此外，雲林擁有著名廟宇吸引萬計人潮，對文化產業的深耕也不容忽視，台灣的布袋戲就是從雲林發跡，揚名於世，讓雲林享有「布袋戲之鄉」的美譽。'
        ],
        link: { label: '雲林旅遊網', url: 'https://tour.yunlin.gov.tw/' },
      },
      '嘉義縣': {
        image: 'assets/img/attraction/city/列表/嘉義縣.jpg',
        heading: '山海交會：三大國家風景區的壯闊',
        paragraphs: [
          '嘉義縣倚山面海，是台灣唯一擁有三大國家風景區的縣份：阿里山、雲嘉南濱海及西拉雅國家風景區，坐擁山色、平原、海景等不同壯麗景觀。',
          '往東是鄒族的原鄉阿里山，有著名的日出、雲海、神木與高山鐵路；往西有東石、布袋等漁港生態，遊客可搭乘觀光漁筏體驗沿海漁業風光。',
          '平原地區則有北回歸線天文廣場，是最佳的戶外教學場所。在好山好水的嘉義縣，您的身心可以在慢遊中獲得充分舒放，體驗魅力嘉義。'
        ],
        link: { label: '嘉義縣文化觀光局', url: 'https://tbocc.cyhg.gov.tw/' },
      },
      '嘉義市': {
        image: 'assets/img/attraction/city/列表/嘉義市.jpg',
        heading: '人文畫都：管樂與交趾陶的藝術城',
        paragraphs: [
          '嘉義市位於嘉南平原北端，開發甚早，是具有歷史淵源的城市。這裡擁有「畫都」美譽，台灣珍貴的「交趾陶」工藝也發源於此，是充滿熱情與藝術的文化之都。',
          '市內景點多與歷史有關，如蘭潭（紅毛埤）是荷蘭人所鑿，如今為市民賞月好去處；嘉義公園與植物園則提供市民活動與避暑的好空間。',
          '近數十年來，嘉義市創造出著名的「石猴」雕刻藝術，每年定期舉辦的管樂節活動更是全國盛事，配合阿里山森林鐵道傳奇，博得國際媒體讚譽。'
        ],
        link: { label: '嘉義市觀光旅遊網', url: 'https://travel.chiayi.gov.tw/' },
      },
      '臺南市': {
        image: 'assets/img/attraction/city/列表/台南.jpg',
        heading: '文化古都：台灣歷史的發祥地',
        paragraphs: [
          '臺南市是臺灣歷史最悠久的都市。西元1661年鄭成功來台後在此開府設治，奠定了都會規模。直到十九世紀末期，臺南一直是臺灣政治經濟文化之重心，古蹟名勝特多，佔有最悠久歷史地位，稱為文化古都。',
          '除了歷史文化，臺南更擁有詩畫般的自然生態美景。春天有聞名的「臺灣國際蘭展」，農曆正月十五更有壯觀的鹽水蜂炮；夏天可前往梅嶺賞螢、白河賞蓮；秋天品嚐東山咖啡；冬天則到關子嶺感受泥漿溫泉。',
          '這般的臺南之美，美得有如活生生的歷史博物館，美在那一望無垠的田園風光與淳樸熱情的人心，值得您前來細細品味。'
        ],
        link: { label: '台南旅遊網', url: 'https://www.twtainan.net/zh-tw/' },
      },
      '高雄市': {
        image: 'assets/img/attraction/city/列表/高雄.jpg',
        heading: '海洋首都：山河港灣的國際港市',
        paragraphs: [
          '高雄市為臺灣南方最繁華的國際城市，受海洋氣候調節，全年陽光普照。近年大力推展觀光，擁有愛河、壽山、西子灣、旗津等知名景點，展現獨特的「海洋首都」特性。',
          '高雄融合了福佬、客家、眷村與原住民等多樣文化，是個集「山、海、河港、人文、古跡」於一身的城市。您可以欣賞美濃紙傘、內門宋江陣，或是造訪佛光山，旅遊元素一應俱全。',
          '高雄人的熱情，就像南方和煦的陽光，誠摯歡迎大家到高雄作客！'
        ],
        link: { label: '高雄旅遊網', url: 'https://khh.travel/zh-tw/' },
      },
      '屏東縣': {
        image: 'assets/img/attraction/city/列表/屏東.jpg',
        heading: '國境之南：四季如春的南洋風情',
        paragraphs: [
          '屏東縣是臺灣最南端的縣，地處熱帶，四季如春，充滿南國風味，有「臺灣南洋」之稱。境內擁有臺灣第一座國家公園——墾丁國家公園，以及海上樂園小琉球。',
          '屏東海邊是休閒玩水的好去處，可浮潛觀賞珊瑚美景，或到鵝鑾鼻感受太平洋與臺灣海峽的交會。此外，這裡也是候鳥過冬的中繼站，是觀賞野鳥生態與浩瀚星空的好地方。',
          '在地美食更是不可錯過，如林邊蓮霧、萬巒豬腳、東港黑鮪魚及恆春洋蔥等。這樣有吃又有得玩的「國境之南」，絕對值得一探究竟。'
        ],
        link: { label: '屏東旅遊網', url: 'https://www.i-pingtung.com/' },
      },
      '花蓮縣': {
        image: 'assets/img/attraction/city/列表/花蓮.jpg',
        heading: '洄瀾山海：世界級的福爾摩沙美景',
        paragraphs: [
          '花蓮古稱奇萊，又名洄瀾。它是臺灣最大的縣份，東臨太平洋，西倚中央山脈。1590年葡萄牙人航經此地，驚呼「FORMOSA」，從此花蓮的壯闊景緻便成為臺灣的代表。',
          '花蓮以巍峨高山、蔚藍天空、浩瀚海洋與親切的人民，成為全國旅遊首選。縣內幾乎全境位於國家風景區中，包含太魯閣國家公園、玉山國家公園、東部海岸及花東縱谷。',
          '如此依山傍海的優美環境，造就了無數世界級的山水美景，是深受國際遊客喜愛的旅遊聖地。'
        ],
        link: { label: '花蓮觀光資訊網', url: 'https://tour-hualien.hl.gov.tw/' },
      },
      '臺東縣': {
        image: 'assets/img/attraction/city/列表/台東.jpg',
        heading: '自然樂活：純真質樸的知性饗宴',
        paragraphs: [
          '臺東擁有豐富的生態資源，海岸、高山與溪谷皆保持自然風貌。物產豐饒，如縱谷好米、金針花、洛神花及臺東釋迦都遠近馳名；漁產則以旗魚與柴魚片最具盛名。',
          '臺東充滿多族群的人文特色，在寬闊的大地上可以享受悠閒的生活步調，聽聞有趣的歷史神話，對每一位旅人而言，都是一場豐收的知性饗宴。',
          '臺東更是單車族的聖地，擁有全臺完善的自行車道網絡，如關山、池上與台東市等。騎行其間，宛若進入純真自然的仙境一般。'
        ],
        link: { label: '台東觀光旅遊網', url: 'https://tour.taitung.gov.tw/zh-tw' },
      },
      '澎湖縣': {
        image: 'assets/img/attraction/city/列表/澎湖.jpg',
        heading: '菊島風情：上帝的石雕公園',
        paragraphs: [
          '澎湖群島遍布玄武岩地質景觀，被譽為「上帝的石雕公園」。清澈的澎湖灣與優質海域，使其成為國際風帆船選手的年度盛事地，媲美加勒比海。',
          '春夏有熱情浪漫的花火節，秋冬則有菊島海鮮節。這裡有碧海藍天與細緻沙灘，各式水上遊樂設施與歎為觀止的海洋生態，肯定會贏得您的連聲讚嘆！',
          '澎湖對外交通便利，無論空運或海運皆能輕鬆抵達。島嶼遊程加上台灣寶島之旅，讓旅程更加豐富多元。'
        ],
        link: { label: '澎湖旅遊網', url: 'https://penghutravel.com/' },
      },
      '金門縣': {
        image: 'assets/img/attraction/city/列表/金門.jpg',
        heading: '戰地遺風：生態與文化的香醇金門',
        paragraphs: [
          '金門擁有豐富的候鳥資源與保育類動物如水獺、鱟。在觀光潮流下，結合產業與文化，最有名的特產莫過於「鋼刀、貢糖、高粱酒」，以及麵線與陶瓷。',
          '由於獨特的水質與氣候，金門高粱酒香醇度冠絕他處。而傳統閩南小吃如蚵仔煎、廣東粥、包餡燒餅等，更是饕客極為喜愛之美味。',
          '結合了戰地歷史、閩南建築與自然生態，金門呈現出獨具特色的鄉土風味。'
        ],
        link: { label: '金門觀光旅遊網', url: 'https://kinmen.travel/zh-tw' },
      },
      '連江縣(馬祖)': {
        image: 'assets/img/attraction/city/列表/連江.jpg',
        heading: '馬祖列島：臨海山城的擺暝傳說',
        paragraphs: [
          '馬祖夏季是賞燕鷗的好時節，神話之鳥「黑嘴端鳳頭燕鷗」更是慕名而來的焦點；冬季則有盛大的元宵「擺暝」祭神活動，是當地最熱鬧的民俗慶典。',
          '馬祖的傳統聚落如芹壁、津沙，其建材與風格迥異於台灣本島，依山傍海的景致令人流連。此外，馬祖也是海釣天堂，四季皆有豐富漁獲。',
          '名產以馬祖老酒及東湧陳高最為遠近馳名，還有紅糟料理與各式海鮮加工品，是極具特色的伴手禮。'
        ],
        link: { label: '馬祖旅遊網', url: 'https://www.matsu.gov.tw/' },
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
