import type { ModCommentContentType } from '../enums/ModCommentContentType';

export interface ModCommentModel {
  id: number;
  author: string;
  comment: string;
  timeCreatedUtc: string;
  contentType: ModCommentContentType;
}
