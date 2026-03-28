import { Component, EventEmitter, inject, Input, OnDestroy, Output } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { PersonalCommentService } from '../../Service/personal-comment-service';

@Component({
  selector: 'app-user-comment-form',
  imports: [ReactiveFormsModule],
  templateUrl: './user-comment-form.html',
  styleUrl: './user-comment-form.css',
})
export class UserCommentForm implements OnDestroy {
  @Input() isOpen = false;
  @Input() activityId = 0;
  @Output() closed = new EventEmitter<void>();
  @Output() commentEmit = new EventEmitter<void>();

  personalCommentService = inject(PersonalCommentService);

  imagePreviews: string[] = [];

  ngOnDestroy(): void {
    this.clearPreviewUrls();
  }

  commentForm = new FormGroup({
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
    }),
    images: new FormControl<File[]>([], {
      nonNullable: true,
    })
  });

  restForm(): void {
    this.commentForm.reset({
      title: '',
      comment: '',
      rating: 0,
      images: []
    });
    this.clearPreviewUrls();
    this.imagePreviews = [];

  }

  closeModal(): void {
    this.closed.emit();
    this.restForm();
  }

  setRating(star: number): void {
    this.commentForm.patchValue({
      rating: star
    });
  }

  isStarActive(star: number): boolean {
    return (this.commentForm.get('rating')?.value ?? 0) >= star;
  }

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files) return;

    const files = Array.from(input.files);

    this.clearPreviewUrls();

    this.imagePreviews = files.map(file => URL.createObjectURL(file));

    this.commentForm.patchValue({
      images: files
    });

    input.value = '';
  }

  removeImage(index: number): void {
    const currentFiles = this.commentForm.get('images')?.value ?? [];
    const updatedFiles = currentFiles.filter((_, i) => i !== index);

    if (this.imagePreviews[index]) {
      URL.revokeObjectURL(this.imagePreviews[index]);
    }

    this.imagePreviews = this.imagePreviews.filter((_, i) => i !== index);

    this.commentForm.patchValue({
      images: updatedFiles
    });
  }

  private clearPreviewUrls(): void {
    this.imagePreviews.forEach(url => URL.revokeObjectURL(url));
  }

  submitComment(): void {
    if (this.commentForm.invalid) {
      this.commentForm.markAllAsTouched();
      return;
    }

    this.personalCommentService.postPersonComment({
      activityId: this.activityId,
      title: this.commentForm.controls.title.value,
      comment: this.commentForm.controls.comment.value,
      rating: this.commentForm.controls.rating.value,
      reviewImages: this.commentForm.controls.images.value
    }).subscribe({
      next: (res) => {
        console.log('送出成功', res);
        this.commentEmit.emit();
        this.closed.emit();
        this.restForm();
      },
      error: (err) => { console.log('送出失敗', err); }
    }
    )
  }
}
