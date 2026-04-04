import { TagDTO } from './ArticleData';
import { JournalDetailDTO } from './JournalDetailDTO';
export interface JournalUpDateDTO {
  title?: string,
  cover?: string,
  status: number,
  regionId?: number
  elements?: JournalElementDTO[]
  tags?: TagDTO[];
}

export interface JournalElementDTO {

  page: number,
  posX: number,
  posY: number,
  rotation: number,
  zindex: number,
  elementType: number,
  content: string,
  width: number,
  height: number,


}

