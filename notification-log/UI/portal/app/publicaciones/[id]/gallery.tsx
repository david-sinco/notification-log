"use client";

import { useState } from "react";
import { LlaveMark } from "@/components/llave-mark";

export function Gallery({ photos }: { photos: { fileName: string; url: string }[] }) {
  const [selected, setSelected] = useState(0);
  const [expanded, setExpanded] = useState(false);

  if (photos.length === 0)
    return (
      <div className="gallery-main listing-cover-empty" style={{ display: "flex" }}>
        <LlaveMark size={48} />
      </div>
    );

  const thumbs = expanded ? photos : photos.slice(0, 4);

  return (
    <div className="gallery">
      <div className="gallery-main">
        <img src={photos[selected].url} alt={`Foto ${selected + 1} de ${photos.length}`} />
      </div>
      <div className="gallery-thumbs">
        {thumbs.map((photo, index) => (
          <button
            key={photo.fileName}
            type="button"
            className="thumb"
            aria-label={`Ver foto ${index + 1}`}
            aria-pressed={index === selected}
            onClick={() => setSelected(index)}
          >
            <img src={photo.url} alt="" />
          </button>
        ))}
        {photos.length > 4 && (
          <button type="button" className="btn" onClick={() => setExpanded(!expanded)}>
            {expanded ? "Ver menos fotos" : `Ver las ${photos.length} fotos`}
          </button>
        )}
      </div>
    </div>
  );
}
