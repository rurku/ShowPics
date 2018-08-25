import { } from 'jasmine';
import { inject, TestBed } from '@angular/core/testing';
import { FileService } from './file.service';
import { HttpClient } from '@angular/common/http';

describe('FileService', () => {
  beforeEach(() => {
    const httpClientSpy = jasmine.createSpy('HttpClient');
    TestBed.configureTestingModule({
      providers: [
        FileService,
        { provide: HttpClient, useValue: httpClientSpy }]
    });
  });

  it('should be created', inject([FileService], (service: FileService) => {
    expect(service).toBeTruthy();
  }));
});
