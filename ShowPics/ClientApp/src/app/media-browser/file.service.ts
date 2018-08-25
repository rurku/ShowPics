import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { FileSystemObject } from './file-service-dtos';

@Injectable()
export class FileService {

  private filesUrl = 'api/Files';  // URL to web api

  private static handleError(error: any): Promise<any> {
    console.error('An error occurred', error); // for demo purposes only

    return Promise.reject(error.message || error);
  }

  constructor(private http: HttpClient) { }

  getFiles(): Observable<FileSystemObject> {
    return this.http.get(this.filesUrl)
      .pipe(
        catchError(FileService.handleError)
      );
  }

  getUri(path: string) {
    return "/" + this.filesUrl + "/" + path;
  }
}
