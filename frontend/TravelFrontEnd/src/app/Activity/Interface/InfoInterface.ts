export interface ActivityInfoInterface {
  activityId: number,
  title: string,
  regions: string[],
  types: string[],
  description: string,
  startTime: Date,
  endTime: Date,
  address: string,
  longitude: number,
  latitude: number,
  propaganda: string,
  officialLink: string,
  images: string[]
  commentCount: number,
  sellCount: number,
}
