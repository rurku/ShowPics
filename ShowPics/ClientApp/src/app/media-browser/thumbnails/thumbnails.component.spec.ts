import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ThumbnailsComponent } from './thumbnails.component';
import { FileService } from '../file.service';
import { FileSystemObject } from '../file-service-dtos';

describe('ThumbnailsComponent', () => {
  let component: ThumbnailsComponent;
  let fixture: ComponentFixture<ThumbnailsComponent>;

  beforeEach(async(() => {
    const fileServiceSpy = jasmine.createSpy('FileService');
    TestBed.configureTestingModule({
      declarations: [ThumbnailsComponent],
      providers: [
        { provide: FileService, useValue: fileServiceSpy }
      ],
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ThumbnailsComponent);
    component = fixture.componentInstance;
    const fso = new FileSystemObject();
    fso.children = [];
    component.fileSystemObject = fso;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
