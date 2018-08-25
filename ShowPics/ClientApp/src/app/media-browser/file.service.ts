import { Injectable } from '@angular/core';
import { Headers, Http } from '@angular/http';
import 'rxjs/add/operator/toPromise';
import { FileSystemObject } from './file-service-dtos';

@Injectable()
export class FileService {

  private filesUrl = 'api/Files';  // URL to web api

  private static handleError(error: any): Promise<any> {
    console.error('An error occurred', error); // for demo purposes only

    return Promise.reject(error.message || error);
  }

  constructor(private http: Http) { }

  getFiles(): Promise<FileSystemObject> {
    return this.http.get(this.filesUrl)
      .toPromise()
      .then(response => response.json() as FileSystemObject)
      .catch(FileService.handleError);
  }

  getUri(path: string) {
    return "/" + this.filesUrl + "/" + path;
  }
}
