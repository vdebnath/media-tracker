import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { MediaItem } from "../models/MediaItem";

export class MediaItemService {
    private actionUrl: string = 'http://localhost:5269/api/mediaitems';
    public readonly version: string = '1.0';

    constructor(private http: HttpClient) {}

    public getMediaItems(): Observable<MediaItem[]> {
        return this.http.get<MediaItem[]>(this.actionUrl);
    }

    public getMediaItem(id: number): Observable<MediaItem> {
        return this.http.get<MediaItem>(`${this.actionUrl}/${id}`)
    }

    public addMediaItem(mediaItem: MediaItem): Observable<MediaItem> {
        return this.http.post<MediaItem>(this.actionUrl, mediaItem);
    }

    public updateMediaItem(id: number, mediaItem: MediaItem): Observable<void> {
        return this.http.put<void>(`${this.actionUrl}/${id}`, mediaItem);
    }

    public deleteMediaItem(id: number): Observable<void> {
        return this.http.delete<void>(`${this.actionUrl}/${id}`);
    }
}