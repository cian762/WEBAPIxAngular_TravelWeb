import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DiffItem, VersionCompareResponse, VersionSummary } from '../../interface/analysis-metrics';
import { ItineraryAnalysis } from '../../service/itinerary-analysis';
@Component({
  selector: 'app-version-compare-dialog',
  imports: [CommonModule],
  templateUrl: './version-compare-dialog.html',
  styleUrl: './version-compare-dialog.css',
})
export class VersionCompareDialog implements OnInit {
  @Input() itineraryId!: number;

  private svc = inject(ItineraryAnalysis);

  step: 'select' | 'result' = 'select';
  versions: VersionSummary[] = [];
  selectedA: number | null = null;
  selectedB: number | null = null;
  result?: VersionCompareResponse;
  isLoadingList = true;
  isComparing = false;
  errorMsg?: string;

  ngOnInit(): void {
    this.svc.getVersionList(this.itineraryId).subscribe({
      next: v => {
        console.log('[CompareDialog] 版本清單回傳 =', v);
        this.versions = v; this.isLoadingList = false;
      },
      error: (err) => {
        console.error('[CompareDialog] 版本清單錯誤 =', err);
        this.isLoadingList = false; this.errorMsg = '無法載入版本清單';
      }
    });
  }

  select(versionId: number, slot: 'A' | 'B'): void {
    if (slot === 'A') {
      this.selectedA = versionId;
      if (this.selectedB === versionId) this.selectedB = null;
    } else {
      this.selectedB = versionId;
      if (this.selectedA === versionId) this.selectedA = null;
    }
  }

  get canCompare(): boolean {
    return this.selectedA !== null && this.selectedB !== null;
  }

  doCompare(): void {
    if (!this.canCompare) return;
    this.isComparing = true;
    this.errorMsg = undefined;
    this.svc.compareVersions(this.itineraryId, this.selectedA!, this.selectedB!)
      .subscribe({
        next: r => { this.result = r; this.isComparing = false; this.step = 'result'; },
        error: () => { this.isComparing = false; this.errorMsg = '比較失敗，請稍後再試'; }
      });
  }
  get versionALabel(): string {
    return this.versions.find(v => v.versionId === this.selectedA)?.versionNumber?.toString() ?? '';
  }
  @Output() close = new EventEmitter<void>();
  closeDialog(): void {
    this.close.emit();
  }
  get versionBLabel(): string {
    return this.versions.find(v => v.versionId === this.selectedB)?.versionNumber?.toString() ?? '';
  }
  backToSelect(): void { this.step = 'select'; this.result = undefined; }

  // ── UI 輔助 ──
  sourceLabel(s?: string): string {
    return s === 'AI' ? '🤖 AI' : s === 'Manual' ? '✏️ 手動' : '';
  }

  statusClass(s: string): string {
    return s === 'Active' ? 'status-active' : 'status-archived';
  }

  diffClass(d: DiffItem): string { return `diff-${d.diffType}`; }
  diffIcon(d: DiffItem): string {
    return { added: '＋', removed: '－', changed: '↔' }[d.diffType];
  }

  delta(a: number, b: number): string {
    const d = Math.round((b - a) * 100);
    return d === 0 ? '—' : d > 0 ? `+${d}%` : `${d}%`;
  }

  deltaClass(a: number, b: number, lowerIsBetter = false): string {
    const d = b - a;
    if (Math.abs(d) < .01) return '';
    return (lowerIsBetter ? d < 0 : d > 0) ? 'delta-good' : 'delta-bad';
  }
}
