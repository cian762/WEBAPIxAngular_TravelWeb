import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { RegionGroup, CityCard } from '../attraction.models';

@Component({
  selector: 'app-attractions-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './attractions-home.html',
  styleUrls: ['./attractions-home.css']
})
export class AttractionsHomeComponent {
  activeRegion = 0;

  regions: RegionGroup[] = [
    {
      label: '北部地區',
      description: '您可以探訪臺灣最多元的城市魅力——臺北101與故宮博物院、前往歷史典藏文物欣賞；踏走迪化街老街、九份、淡水等充滿懷舊的歷史勝地。',
      cities: [
        { name: '臺北市',   regionIds: [1001,1002,1003,1004,1005,1006], imageUrl: 'assets/img/package/package1.jpg' },
        { name: '新北市',   regionIds: [1101,1102,1103],                imageUrl: 'assets/img/package/package2.jpg' },
        { name: '基隆市',   regionIds: [1201],                          imageUrl: 'assets/img/package/package3.jpg' },
        { name: '宜蘭縣',   regionIds: [1501],                          imageUrl: 'assets/img/package/package4.jpg' },
        { name: '桃園市',   regionIds: [1301,1302,1303],                imageUrl: 'assets/img/package/package5.jpg' },
        { name: '新竹縣市', regionIds: [1401,1402],                     imageUrl: 'assets/img/package/package6.jpg' },
      ]
    },
    {
      label: '中部地區',
      description: '日月潭的湖光山色、八卦山、梨山、獅頭山等，都可充分感受臺灣多元文化及豐富自然景觀。苗栗、台中、彰化、南投、雲林皆有精彩可期。',
      cities: [
        { name: '苗栗縣', regionIds: [1601], imageUrl: 'assets/img/package/package1.jpg' },
        { name: '台中市', regionIds: [2001,2002,2003,2004,2005], imageUrl: 'assets/img/package/package2.jpg' },
        { name: '彰化縣', regionIds: [2101], imageUrl: 'assets/img/package/package3.jpg' },
        { name: '南投縣', regionIds: [2201], imageUrl: 'assets/img/package/package4.jpg' },
        { name: '雲林縣', regionIds: [2301], imageUrl: 'assets/img/package/package5.jpg' },
      ]
    },
    {
      label: '南部地區',
      description: '探訪濃厚的歷史文化：品嚐傳統美食小吃、一睹世界級日出奇景；也可刺激水上活動，近覽太平洋壯闊景觀。',
      cities: [
        { name: '嘉義縣市', regionIds: [2401,2402], imageUrl: 'assets/img/package/package1.jpg' },
        { name: '台南市',   regionIds: [3101,3102,3103,3104,3105], imageUrl: 'assets/img/package/package2.jpg' },
        { name: '高雄市',   regionIds: [3201,3202,3203], imageUrl: 'assets/img/package/package3.jpg' },
        { name: '屏東縣',   regionIds: [3301], imageUrl: 'assets/img/package/package4.jpg' },
      ]
    },
    {
      label: '東部地區',
      description: '壯觀的太魯閣峽谷、美麗的花東縱谷，以及台東都蘭山、綠島等，是享受大自然洗禮的最佳去處。',
      cities: [
        { name: '花蓮縣', regionIds: [4001], imageUrl: 'assets/img/package/package1.jpg' },
        { name: '台東縣', regionIds: [4101], imageUrl: 'assets/img/package/package2.jpg' },
      ]
    },
    {
      label: '離島地區',
      description: '澎湖、金門、馬祖各具獨特的自然景觀和歷史文化，是尋訪純淨海洋與閩南文化的好去處。',
      cities: [
        { name: '澎湖縣', regionIds: [5001], imageUrl: 'assets/img/package/package1.jpg' },
        { name: '金門縣', regionIds: [5101], imageUrl: 'assets/img/package/package2.jpg' },
        { name: '連江縣', regionIds: [5201], imageUrl: 'assets/img/package/package3.jpg' },
      ]
    },
  ];

  get currentRegion(): RegionGroup { return this.regions[this.activeRegion]; }

  constructor(private router: Router) {}

  selectRegion(i: number): void { this.activeRegion = i; }

  goToCity(city: CityCard): void {
    this.router.navigate(['/contact/list'], {
      queryParams: { regionIds: city.regionIds.join(','), cityName: city.name }
    });
  }
}
