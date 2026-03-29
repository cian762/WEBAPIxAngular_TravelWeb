import {
  Component,
  EventEmitter,
  Input,
  Output,
  OnDestroy,
  OnChanges,
  SimpleChanges,
  inject
} from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { PersonalCommentService } from '../../Service/personal-comment-service';

@Component({
  selector: 'app-edit-comment-form',
  imports: [ReactiveFormsModule],
  templateUrl: './edit-comment-form.html',
  styleUrl: './edit-comment-form.css',
})
export class EditCommentForm implements OnChanges, OnDestroy {
  @Input() isOpen = false;
  @Input() reviewId = 0;
  @Input() initialTitle = '';
  @Input() initialComment = '';
  @Input() initialRating = 0;
  @Input() initialImages: string[] = [];

  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  personalCommentService = inject(PersonalCommentService);

  existingImages: string[] = [];
  deletedImageUrls: string[] = [];

  newImages: File[] = [];
  newImagePreviews: string[] = [];

  isSubmitting = false;
  submitErrorMessage = '';

  editForm = new FormGroup({
    title: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required]
    }),
    comment: new FormControl('', {
      nonNullable: true
    }),
    rating: new FormControl(0, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(1), Validators.max(5)]
    })
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (!this.isOpen) return;

    this.editForm.reset({
      title: this.initialTitle,
      comment: this.initialComment,
      rating: this.initialRating
    });

    this.existingImages = [...this.initialImages];
    this.deletedImageUrls = [];

    this.clearPreviewUrls();
    this.newImages = [];
    this.newImagePreviews = [];

    this.submitErrorMessage = '';
    this.isSubmitting = false;
    this.editForm.enable({ emitEvent: false });
  }

  setRating(star: number): void {
    if (this.isSubmitting) return;
    this.editForm.patchValue({ rating: star });
  }

  isStarActive(star: number): boolean {
    return this.editForm.controls.rating.value >= star;
  }

  onNewImagesChange(event: Event): void {
    if (this.isSubmitting) return;

    const input = event.target as HTMLInputElement;
    if (!input.files) return;

    const files = Array.from(input.files);
    const previewUrls = files.map(file => URL.createObjectURL(file));

    this.newImages = [...this.newImages, ...files];
    this.newImagePreviews = [...this.newImagePreviews, ...previewUrls];

    input.value = '';
  }

  removeExistingImage(index: number): void {
    if (this.isSubmitting) return;

    const targetUrl = this.existingImages[index];
    if (!targetUrl) return;

    this.deletedImageUrls.push(targetUrl);
    this.existingImages = this.existingImages.filter((_, i) => i !== index);
  }

  removeNewImage(index: number): void {
    if (this.isSubmitting) return;

    const previewUrl = this.newImagePreviews[index];
    if (previewUrl) {
      URL.revokeObjectURL(previewUrl);
    }

    this.newImages = this.newImages.filter((_, i) => i !== index);
    this.newImagePreviews = this.newImagePreviews.filter((_, i) => i !== index);
  }

  submitEdit(): void {
    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }

    if (this.isSubmitting) return;

    this.isSubmitting = true;
    this.submitErrorMessage = '';
    this.editForm.disable({ emitEvent: false });

    this.personalCommentService.patchPersonalComment({
      reviewId: this.reviewId,
      title: this.editForm.controls.title.value,
      comment: this.editForm.controls.comment.value,
      rating: this.editForm.controls.rating.value,
      newImages: this.newImages,
      deletedImageUrls: this.deletedImageUrls
    })
      .pipe(
        finalize(() => {
          this.isSubmitting = false;
          this.editForm.enable({ emitEvent: false });
        })
      )
      .subscribe({
        next: (res) => {
          console.log('修改成功', res);
          this.saved.emit();
          this.closed.emit();
        },
        error: (err) => {
          console.log('修改失敗', err);
          this.submitErrorMessage = '修改失敗，請稍後再試一次';
        }
      });
  }

  closeModal(): void {
    if (this.isSubmitting) return;
    this.closed.emit();
  }

  private clearPreviewUrls(): void {
    this.newImagePreviews.forEach(url => URL.revokeObjectURL(url));
  }

  ngOnDestroy(): void {
    this.clearPreviewUrls();
  }
}
