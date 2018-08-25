import { Component, OnInit, Input } from '@angular/core';
import { FileSystemObject, FileSystemObjectTypes } from '../file-service-dtos';
import { FileService } from '../file.service';
import * as PhotoSwipe from 'photoswipe';
import * as PhotoSwipeUI_Default from 'photoswipe/dist/photoswipe-ui-default'



@Component({
  selector: 'app-thumbnails',
  templateUrl: './thumbnails.component.html',
  styleUrls: ['./thumbnails.component.css']
})
export class ThumbnailsComponent implements OnInit {

  constructor(public fileService: FileService) { }

  @Input() fileSystemObject: FileSystemObject;

  ngOnInit() {
  }

  onSelect(file: FileSystemObject): void {
    var pswpElement = document.querySelectorAll('.pswp')[0] as HTMLElement;

    // build items array
    var items : PhotoSwipe.Item[] = this.getFiles().map(f =>
      {
        return {
          src: f.path,
          w: f.width,
          h: f.height
        } as PhotoSwipe.Item;
      });

    // define options (if needed)
    var options : PhotoSwipe.Options = {
      // optionName: 'option value'
      // for example:
      index: this.getFiles().findIndex(f => f.path === file.path)
    };

    // Initializes and opens PhotoSwipe
    var gallery = new PhotoSwipe(pswpElement, PhotoSwipeUI_Default, items, options);
    gallery.init();
  }
  
  public getFiles(): FileSystemObject[] {
    return this.fileSystemObject.children.filter(child => child.type == FileSystemObjectTypes.FILE);
  }
}
