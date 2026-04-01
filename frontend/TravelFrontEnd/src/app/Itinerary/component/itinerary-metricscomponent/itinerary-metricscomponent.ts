import { Component, Input, OnInit, SimpleChanges, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { AnalysisMetrics } from '../../interface/analysis-metrics';
import { ItineraryAnalysis } from '../../service/itinerary-analysis';
interface RingConfig {
  label: string;
  value: number;
  color: string;
  hint: string;
  // 疲勞度：越低越好，顏色邏輯反向
  invert?: boolean;
}

@Component({
  selector: 'app-itinerary-metricscomponent',
  imports: [DatePipe],
  templateUrl: './itinerary-metricscomponent.html',
  styleUrl: './itinerary-metricscomponent.css',
})
export class ItineraryMetricscomponent implements OnInit {
  @Input() itineraryId!: number;
  @Input() versionId!: number;

  private svc = inject(ItineraryAnalysis);

  isLoading = true;
  _analysis?: AnalysisMetrics;
  rings: RingConfig[] = [];

  readonly R = 48;
  readonly C = +(2 * Math.PI * 48).toFixed(2);
ngOnChanges(changes: SimpleChanges): void {
    // 當 versionId 有值且不為 0 才呼叫
    if (this.itineraryId && this.versionId) {
      console.log('[Metrics] Input 變化，呼叫 API itineraryId =',
        this.itineraryId, 'versionId =', this.versionId);
      this.loadAnalysis();
    }
  }
  private loadAnalysis(): void {
    this.isLoading = true;
    this._analysis = undefined;

    this.svc.getAnalysis(this.itineraryId, this.versionId).subscribe({
      next: data => {
        console.log('[Metrics] API 回傳 =', data);
        this._analysis = data;
        this.buildRings(data);
        this.isLoading = false;
      },
      error: err => {
        console.error('[Metrics] API 錯誤 =', err);
        this.isLoading = false;
      }
    });
  }
  ngOnInit(): void {
    console.log('[Metrics] 呼叫參數 itineraryId =', this.itineraryId, 'versionId =', this.versionId);

    this.svc.getAnalysis(this.itineraryId, this.versionId).subscribe({
      next: data => {
        console.log('[Metrics] API 回傳 =', data);
        this._analysis = data;
        this.buildRings(data);
        this.isLoading = false;
      },
      error: err => {
        console.error('[Metrics] API 錯誤 =', err);
        this.isLoading = false;
      }
    });
  }

  offset(v: number): number {
    return +(this.C * (1 - Math.min(Math.max(v, 0), 1))).toFixed(2);
  }

  private buildRings(a: AnalysisMetrics): void {
    this.rings = [
      {
        label: '可行度', value: a.feasibilityScore,
        color: this.scoreColor(a.feasibilityScore, false),
        hint: a.feasibilityScore >= .75 ? '✅ 行程流暢'
          : a.feasibilityScore >= .5 ? '⚠️ 略需調整' : '❌ 建議重排'
      },
      {
        label: '步調平衡', value: a.paceBalanceScore,
        color: this.scoreColor(a.paceBalanceScore, false),
        hint: a.paceBalanceScore >= .75 ? '👍 節奏均衡'
          : a.paceBalanceScore >= .5 ? '🔄 略有起伏' : '⚡ 安排不均'
      },
      {
        label: '疲勞指數', value: a.fatigueIndex,
        color: this.scoreColor(a.fatigueIndex, true),
        hint: a.fatigueIndex <= .4 ? '😊 輕鬆愜意'
          : a.fatigueIndex <= .7 ? '😐 尚可接受' : '😩 行程偏累',
        invert: true
      }
    ];
  }

  private scoreColor(v: number, invert: boolean): string {
    const good = invert ? v <= .4 : v >= .75;
    const warn = invert ? v <= .7 : v >= .5;
    return good ? '#22c55e' : warn ? '#f59e0b' : '#ef4444';
  }

  pct(v: number): string { return `${Math.round(v * 100)}`; }
}
