import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'media-item-nav',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './media-item-nav.html',
  styleUrl: './media-item-nav.scss',
})
export class MediaItemNav {}
