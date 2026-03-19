import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { AttractionService } from '../attraction.service';
import { SafeUrlPipe } from '../safe-url.pipe';
import { Attraction } from '../attraction.models';

@Component({
  selector: 'app-attraction-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, SafeUrlPipe],
  templateUrl: './attraction-detail.html',
  styleUrls: ['./attraction-detail.css']
})
export class AttractionDetailComponent implements OnInit {
  attraction: Attraction | null = null;
  loading = true;
  activeTab = 'feature';
  currentImgIdx = 0;

  tabs = [
    { key: 'feature',    label: '景點特色', icon: '🏞️' },
    { key: 'transport',  label: '如何前往', icon: '🚌' },
    { key: 'accessible', label: '友善指引', icon: '♿' },
    { key: 'nearby',     label: '周邊資訊', icon: '📍' },
  ];

  weather = { temp: 24, rain: 10, aqi: 35, aqiLabel: '良好' };

  constructor(
    private route: ActivatedRoute,
    private svc: AttractionService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(p => {
      const id = Number(p.get('id'));
      if (id) {
        this.svc.getAttractionById(id).subscribe(data => {
          this.attraction = data;
          this.loading = false;
        });
      }
    });
  }

  get mainImage(): string {
    if (this.attraction?.images?.length) {
      return `http://localhost:7276${this.attraction.images[this.currentImgIdx].imagePath}`;
    }
    return 'assets/img/package/package2.jpg';
  }

  get imageCount(): number { return this.attraction?.images?.length ?? 0; }

  prevImg(): void {
    this.currentImgIdx = (this.currentImgIdx - 1 + this.imageCount) % this.imageCount;
  }
  nextImg(): void {
    this.currentImgIdx = (this.currentImgIdx + 1) % this.imageCount;
  }

  toggleLike(): void {
    if (!this.attraction) return;
    this.svc.toggleLike(this.attraction.attractionId).subscribe(res => {
      if (this.attraction) {
        this.attraction.likeCount = res.likeCount;
        this.attraction.isLiked   = res.isLiked;
      }
    });
  }

  openNav(): void {
    if (!this.attraction) return;
    window.open(
      `https://www.google.com/maps/dir/?api=1&destination=${this.attraction.latitude},${this.attraction.longitude}`,
      '_blank'
    );
  }

  get mapUrl(): string {
    const lat = this.attraction?.latitude ?? 25.0;
    const lng = this.attraction?.longitude ?? 121.5;
    return `https://maps.google.com/maps?q=${lat},${lng}&z=15&output=embed`;
  }
}
