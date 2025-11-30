import { CommonModule } from '@angular/common';
import { Component, ElementRef, EventEmitter, inject, Input, Output, ViewChild } from '@angular/core';
import { Storage, ref, uploadBytesResumable, getDownloadURL } from '@angular/fire/storage';
import { AuthService } from '../../services/auth.service';
import { GymService } from '../../services/gym.service';





@Component({
  selector: 'app-add-images',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './add-images.component.html',
  styleUrls:  ['./add-images.component.css'],

})
export class AddImagesComponent {

  @Input() gymId!: number;
  @Output() close = new EventEmitter<void>();
  @Output() imagesUploaded = new EventEmitter<string[]>();

  @ViewChild('fileInput') fileInputRef!: ElementRef<HTMLInputElement>;
  private storage = inject(Storage);

  selectedFiles: File[] = [];
  imagePreviews: string[] = [];
  uploadedUrls: string[] = [];

  constructor(private gymservice:GymService) {}


  onFileSelected(event: any) {
    const files = Array.from(event.target.files) as File[];

    this.selectedFiles = files;
    this.imagePreviews = [];

    files.forEach(file => {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.imagePreviews.push(e.target.result);
      };
      reader.readAsDataURL(file);
    });
  }

   async onSubmit() {

    if (this.selectedFiles.length === 0) return;
     this.uploadedUrls = [];
      for (const file of this.selectedFiles) {
        const filePath = `gyms/${this.gymId}/${Date.now()}-${file.name}`;
    const storageRef = ref(this.storage, filePath);

     const uploadTask = uploadBytesResumable(storageRef, file);

      await new Promise<void>((resolve, reject) => {
        uploadTask.on(
          'state_changed',
          () => {},
          reject,
          async () => {
            const url = await getDownloadURL(storageRef);
            this.uploadedUrls.push(url);
            resolve();
          }
        );
      });
    }
 this.gymservice.uploadGymImages(this.gymId, this.uploadedUrls)
    .subscribe({
      next: (res) => {
        console.log('Images saved in database:', res);
        this.imagesUploaded.emit(this.uploadedUrls);
        this.resetForm();
        this.close.emit();
      },
      error: (err) => console.error('Error saving image URLs:', err)
    });
  }

  onCancel() {
    this.resetForm();
    this.close.emit();
  }

  // Helper to reset the component state
  private resetForm() {
    this.selectedFiles = [];
    this.imagePreviews = [];
    this.uploadedUrls = [];
     if (this.fileInputRef) {
      this.fileInputRef.nativeElement.value = ''; // ✅ reset file input itself
    }

  }
}
