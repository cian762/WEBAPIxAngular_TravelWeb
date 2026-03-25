import { Component } from '@angular/core';

@Component({
  selector: 'app-travelindex',
  imports: [],
  templateUrl: './travelindex.html',
  styleUrl: './travelindex.css',
})
export class Travelindex {
  favoriteList = [
    {
      id: 1,
      title: '九份山城散策',
      region: '新北',
      category: '景點',
      description: '老街、茶樓與山海景色一次收集。',
      imageUrl: 'https://images.unsplash.com/photo-1526481280695-3c4691dbbb72?auto=format&fit=crop&w=900&q=80'
    },
    {
      id: 2,
      title: '花蓮海岸線一日遊',
      region: '花蓮',
      category: '行程',
      description: '山海交會的經典路線，適合放鬆出走。',
      imageUrl: 'https://images.unsplash.com/photo-1506744038136-46273834b3fb?auto=format&fit=crop&w=900&q=80'
    },
    {
      id: 3,
      title: '台南古城美食路線',
      region: '台南',
      category: '文章',
      description: '從牛肉湯到老屋巷弄，一次吃遍。',
      imageUrl: 'https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?auto=format&fit=crop&w=900&q=80'
    }
  ];

  articleList = [
    {
      id: 1,
      tag: '旅遊攻略',
      title: '2026 台北兩天一夜這樣玩',
      summary: '整合景點、美食與交通建議，適合第一次來台北的旅人。',
      date: '2026-03-25',
      imageUrl: 'https://images.unsplash.com/photo-1513407030348-c983a97b98d8?auto=format&fit=crop&w=900&q=80'
    },
    {
      id: 2,
      tag: '季節主題',
      title: '春季賞花景點推薦',
      summary: '精選北中南東值得安排的花季路線。',
      date: '2026-03-21',
      imageUrl: 'https://images.unsplash.com/photo-1462275646964-a0e3386b89fa?auto=format&fit=crop&w=900&q=80'
    },
    {
      id: 3,
      tag: '在地體驗',
      title: '夜市美食探索地圖',
      summary: '從經典小吃到新派創意點心，吃貨必收。',
      date: '2026-03-18',
      imageUrl: 'https://images.unsplash.com/photo-1555396273-367ea4eb4db5?auto=format&fit=crop&w=900&q=80'
    }
  ];

  eventList = [
    {
      id: 1,
      title: '阿里山日出鐵道體驗',
      location: '嘉義',
      description: '搭配林鐵與山林景觀的熱門活動。',
      score: 4.8,
      price: 1680,
      imageUrl: 'https://images.unsplash.com/photo-1500530855697-b586d89ba3ee?auto=format&fit=crop&w=900&q=80'
    },
    {
      id: 2,
      title: '宜蘭溫泉放鬆小旅行',
      location: '宜蘭',
      description: '泡湯、美食、慢旅行一次滿足。',
      score: 4.7,
      price: 1380,
      imageUrl: 'https://images.unsplash.com/photo-1519046904884-53103b34b206?auto=format&fit=crop&w=900&q=80'
    },
    {
      id: 3,
      title: '墾丁海上活動體驗',
      location: '屏東',
      description: '適合喜歡陽光與海洋的玩家。',
      score: 4.9,
      price: 2200,
      imageUrl: 'https://images.unsplash.com/photo-1507525428034-b723cf961d3e?auto=format&fit=crop&w=900&q=80'
    },
    {
      id: 4,
      title: '九族文化村親子遊',
      location: '南投',
      description: '適合家庭客群的熱門體驗。',
      score: 4.6,
      price: 980,
      imageUrl: 'https://images.unsplash.com/photo-1527631746610-bca00a040d60?auto=format&fit=crop&w=900&q=80'
    }
  ];

  attractionList = [
    {
      id: 1,
      title: '台北 101',
      region: '北部',
      imageUrl: 'https://images.unsplash.com/photo-1513407030348-c983a97b98d8?auto=format&fit=crop&w=900&q=80'
    },
    {
      id: 2,
      title: '日月潭',
      region: '中部',
      imageUrl: 'https://images.unsplash.com/photo-1501785888041-af3ef285b470?auto=format&fit=crop&w=900&q=80'
    },
    {
      id: 3,
      title: '安平古堡',
      region: '南部',
      imageUrl: 'https://images.unsplash.com/photo-1494526585095-c41746248156?auto=format&fit=crop&w=900&q=80'
    },
    {
      id: 4,
      title: '太魯閣峽谷',
      region: '東部',
      imageUrl: 'https://images.unsplash.com/photo-1500534314209-a25ddb2bd429?auto=format&fit=crop&w=900&q=80'
    }
  ];

  packageList = [
    {
      id: 1,
      title: '花東三日精華套裝',
      days: '3 天 2 夜',
      description: '結合海岸、公路、美食與住宿安排的輕鬆路線。',
      tags: ['含住宿', '親子適合', '熱門路線'],
      score: 4.8,
      price: 6990,
      imageUrl: 'https://images.unsplash.com/photo-1476514525535-07fb3b4ae5f1?auto=format&fit=crop&w=900&q=80'
    },
    {
      id: 2,
      title: '阿里山日月潭經典套裝',
      days: '2 天 1 夜',
      description: '山景與湖景雙主題的人氣經典組合。',
      tags: ['自然景觀', '交通便利', '熱銷'],
      score: 4.7,
      price: 4880,
      imageUrl: 'https://images.unsplash.com/photo-1500534314209-a25ddb2bd429?auto=format&fit=crop&w=900&q=80'
    },
    {
      id: 3,
      title: '台南高雄南部文化行',
      days: '3 天 2 夜',
      description: '老城、美食、港灣風景一次打包。',
      tags: ['美食', '古蹟', '城市漫遊'],
      score: 4.6,
      price: 5590,
      imageUrl: 'https://images.unsplash.com/photo-1482192505345-5655af888cc4?auto=format&fit=crop&w=900&q=80'
    }
  ];

  serviceList = [
    { icon: '✈️', title: '交通資訊', desc: '機場、鐵路、高鐵與轉乘資訊' },
    { icon: '🧾', title: '旅遊須知', desc: '簽證、退稅、常見問題整理' },
    { icon: '🗺️', title: '旅遊地圖', desc: '快速查看地區與熱門景點' },
    { icon: '📶', title: '旅遊便利', desc: 'Wi-Fi、支付與旅客服務資訊' }
  ];
}
