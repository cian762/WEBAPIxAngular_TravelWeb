import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { reviewResponseDTO } from '../Interface/reviewResonseDTO';
import { Observable } from 'rxjs';
import { postPersonalReviewRequest } from '../Interface/postPersonalReviewRequest';
import { editPersonalReviewRequest } from '../Interface/editPersonalReviewRequset';
import { form } from '@angular/forms/signals';

@Injectable({
  providedIn: 'root',
})
export class PersonalCommentService {

  http = inject(HttpClient);
  baseUrl: string = "https://localhost:7276/api/PersonalReview";

  getPersonalComments(activityId: number): Observable<reviewResponseDTO[]> {
    return this.http.get<reviewResponseDTO[]>(`https://localhost:7276/api/PersonalReview/${activityId}`)
  }

  postPersonComment(requset: postPersonalReviewRequest) {
    //將 request 包成 FormData 送進 API
    const formData = new FormData();
    formData.append('activityId', requset.activityId.toString());
    formData.append('title', requset.title);
    formData.append('comment', requset.comment);
    formData.append('rating', requset.rating.toString());

    //多張圖片用同一個 key (即 reviewImages 這個key) 重複 append
    requset.reviewImages.forEach(file => { formData.append('reviewImages', file) });

    return this.http.post(this.baseUrl, formData);
  }

  patchPersonalComment(request: editPersonalReviewRequest) {
    const formData = new FormData();
    formData.append('reviewId', request.reviewId.toString());
    formData.append('title', request.title);
    formData.append('comment', request.comment);
    formData.append('rating', request.rating.toString());

    request.newImages.forEach(file => { formData.append('newImages', file) });
    request.deletedImageUrls.forEach(url => { formData.append('deletedImageUrls', url) });

    return this.http.patch(this.baseUrl, formData);

  }

  deletePresonalComment(reviewId: number) {
    const params = new HttpParams().set('reviewId', reviewId);
    return this.http.delete(this.baseUrl, { params });
  }

}
