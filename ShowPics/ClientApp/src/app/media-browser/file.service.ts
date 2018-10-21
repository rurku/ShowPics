import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { FileSystemObject } from './file-service-dtos';

@Injectable()
export class FileService {

  public static readonly rootUrl = '/api/Files';  // URL to web api

  private static handleError(error: any): Promise<any> {
    console.error('An error occurred', error); // for demo purposes only

    return Promise.reject(error.message || error);
  }

  constructor(private http: HttpClient) { }

  getFiles(path: string, depth: number): Observable<FileSystemObject> {
    return this.http.get(path, {
      params: {
        depth: depth.toString()
      }
    })
      .pipe(
        catchError(FileService.handleError)
      );
  }
}
