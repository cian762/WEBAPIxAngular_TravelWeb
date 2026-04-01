export interface AnalysisMetrics {
  analysisId: number;
  versionId: number;
  feasibilityScore: number;  // 0~1
  paceBalanceScore: number;  // 0~1
  fatigueIndex: number;  // 0~1
  reason: string;
  analysisTime: string;
}
export interface VersionSummary {
  versionId: number;
  versionNumber: number;
  source?: string;
  versionRemark?: string;
  createTime: string;
  currentUsageStatus: string;
  hasAnalysis: boolean;
}

export interface ItemSnap {
  dayNumber: number;
  sortOrder: number;
  attractionName: string;
  startTime?: string;
  endTime?: string;
  googlePlaceId?: string;
}

export interface VersionDetail {
  versionId: number;
  versionNumber: number;
  createTime: string;
  versionRemark?: string;
  items: ItemSnap[];
  analysis?: AnalysisMetrics;
}

export interface DiffItem {
  label: string;
  oldValue?: string;
  newValue?: string;
  diffType: 'added' | 'removed' | 'changed';
}

export interface VersionCompareResponse {
  comparisonId: number;
  versionA: VersionDetail;
  versionB: VersionDetail;
  diffs: DiffItem[];
}
